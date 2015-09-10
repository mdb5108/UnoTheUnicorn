using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.DebugView;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Graphics;
using MonoGame_Test;
using pony;
using Game2;

using xTile;

namespace levels
{
    class Levels
    {

        public Walls _floor;            // body that is effected by physics
        public Walls _rightwall;
        public Walls _leftwall;
        public Walls _ceiling;

        private static readonly float[] BALLOON_SPEEDS = {1f, 2f, 3f};

        private static Levels levelInstance;

        public List<Walls> walls = new List<Walls>();

        private static readonly float TILE_SIZE = 32f;
        private static readonly float TILE_SIZE_CONV = ConvertUnits.ToSimUnits(TILE_SIZE);

        private Unicorn Uno;

        private static readonly string[] levels = {
            "Content\\Level3.tbin",
            "Content\\Map1.tbin",
            "Content\\DemoLevelConcept3.tbin",
        };
        public static int levelCount
        {
            get {return levels.Length;}
        }

        private enum TileDirection {COMBINED, HORIZONTAL, VERTICAL, UNDEFINED};
        class TileAggregate
        {
            public string type;
            public string color;
            public TileDirection dir;
            public Rectangle rect;
            public Dictionary<string, string> properties = new Dictionary<string, string>();
        }

        public static Levels getInstance()
        {

            if (levelInstance == null)
                levelInstance = new Levels();

            return levelInstance;
        }

        public void Initialize(uint level,World world, ContentManager content, out Map map, out Vector2 unoPos)
        {
            foreach(var wall in walls)
            {
                wall.Destroy();
            }

            var stream = TitleContainer.OpenStream(levels[level]);
            map = xTile.Format.FormatManager.Instance.BinaryFormat.Load(stream);

            ParseMap(ref map, world, content, out unoPos);
        }

        private void ParseMap(ref Map map, World world, ContentManager content, out Vector2 unoPos)
        {
            var layerCount = map.Layers.Count;
            Vector2 tempUnoPos = new Vector2();

            xTile.Layers.Layer platformLayer = map.Layers[layerCount-7];
            xTile.Layers.Layer moveablePlatformLayer = map.Layers[layerCount-6];
            xTile.Layers.Layer zoneLayer = map.Layers[layerCount-5];
            xTile.Layers.Layer startLayer = map.Layers[layerCount-4];
            xTile.Layers.Layer balloonLayer = map.Layers[layerCount-3];
            xTile.Layers.Layer speedLayer = map.Layers[layerCount-2];
            xTile.Layers.Layer triggerLayer = map.Layers[layerCount-1];

            ParseTiles(platformLayer, delegate(List<TileAggregate> aggregates, Dictionary<Point, TileAggregate> pointToAggregate)
            {
                foreach(var aggregate in aggregates)
                {
                    if(aggregate.type == "Wall" || aggregate.type == "Magnetic Wall")
                    {
                        Rectangle rect = aggregate.rect;
                        walls.Add(new Walls(aggregate.color, world, (uint)rect.Width, (uint)rect.Height, new Point(rect.X, rect.Y), true));
                    }
                }
            });

            ParseTiles(startLayer, delegate(List<TileAggregate> aggregates, Dictionary<Point, TileAggregate> pointToAggregate)
            {
                foreach(var aggregate in aggregates)
                {
                    if(aggregate.type == "Uno")
                    {
                        float tileSize = GameManager.TILE_SIZE;
                        Rectangle rect = aggregate.rect;
                        tempUnoPos = new Vector2(rect.X*tileSize, rect.Y*tileSize);
                    }
                }
            });

            Dictionary<Point, TileAggregate> pointToZoneAggregate = null;
            ParseTiles(zoneLayer, delegate(List<TileAggregate> aggregates, Dictionary<Point, TileAggregate> pointToAggregate)
            {
                pointToZoneAggregate = pointToAggregate;
            });

            Dictionary<Point, TileAggregate> pointToSpeedAggregate = null;
            ParseTiles(speedLayer, delegate(List<TileAggregate> aggregates, Dictionary<Point, TileAggregate> pointToAggregate)
            {
                pointToSpeedAggregate = pointToAggregate;
            });

            Dictionary<string, List<IActivateable>> activators = new Dictionary<string, List<IActivateable>>();
            Dictionary<Point, TileAggregate> pointToTriggerAggregate = null;
            ParseTiles(triggerLayer, delegate(List<TileAggregate> aggregates, Dictionary<Point, TileAggregate> pointToAggregate)
            {
                pointToTriggerAggregate = pointToAggregate;
            });

            ParseTiles(moveablePlatformLayer, delegate(List<TileAggregate> aggregates, Dictionary<Point, TileAggregate> pointToAggregate)
            {
                foreach(var aggregate in aggregates)
                {
                    if(aggregate.type == "Wall" || aggregate.type == "Magnetic Wall")
                    {
                        Rectangle rect = aggregate.rect;

                        Point origin = rect.Location;
                        Point destination = origin;
                        TileAggregate zone;
                        if(pointToZoneAggregate.TryGetValue(origin, out zone))
                        {
                            bool zoneIsWide = zone.rect.Width > zone.rect.Height;
                            if(zone.rect.Location == origin)
                            {
                                if(zoneIsWide)
                                    origin = new Point(zone.rect.Right-rect.Width, rect.Y);
                                else
                                    origin = new Point(rect.X, zone.rect.Bottom-rect.Height);
                            }
                            else
                            {
                                if(zoneIsWide)
                                    origin = new Point(zone.rect.X, rect.Y);
                                else
                                    origin = new Point(rect.X, zone.rect.Y);
                            }
                        }

                        MovableWall wall = new MovableWall(aggregate.color, world, (uint)rect.Width, (uint)rect.Height, origin, 5f, destination);

                        walls.Add(wall);
                        TileAggregate trigger;
                        if(pointToTriggerAggregate.TryGetValue(rect.Location, out trigger))
                        {
                            string triggerNumber = trigger.properties["Number"];
                            if(!activators.ContainsKey(triggerNumber))
                                    activators[triggerNumber] = new List<IActivateable>();
                            activators[triggerNumber].Add(wall);
                        }
                    }
                }
            });

            ParseTiles(balloonLayer, delegate(List<TileAggregate> aggregates, Dictionary<Point, TileAggregate> pointToAggregate)
            {
                foreach(var aggregate in aggregates)
                {
                    if(aggregate.type == "Balloon")
                    {
                        Rectangle rect = aggregate.rect;

                        float speed = 0f;
                        int range = 0;

                        //If yellow set speed and range
                        if(aggregate.color == "y")
                        {
                            speed = 1f;
                            TileAggregate speedAggregate;
                            string speedName;
                            if(pointToSpeedAggregate.TryGetValue(new Point(rect.X, rect.Y), out speedAggregate)
                                && speedAggregate.type == "Speed Modifier"
                                && speedAggregate.properties.TryGetValue("Speed", out speedName))
                            {
                                switch(speedName)
                                {
                                    default:
                                    case "Slow":
                                        speed = BALLOON_SPEEDS[0];
                                        break;
                                    case "Med":
                                        speed = BALLOON_SPEEDS[1];
                                        break;
                                    case "Fast":
                                        speed = BALLOON_SPEEDS[2];
                                        break;
                                }
                            }

                            TileAggregate zone;
                            if(pointToZoneAggregate.TryGetValue(new Point(rect.X, rect.Y), out zone)
                                && zone.color == "r")
                            {
                                //Start at left side
                                rect.X = zone.rect.X;
                                rect.Y = zone.rect.Y;
                                //Go for horizontal width of zone
                                range = zone.rect.Width;
                            }

                        }

                        Balloon balloon;
                        if(aggregate.color != "g")
                        {
                            balloon = new Balloon(new Point(rect.X, rect.Y), content, aggregate.color, speed, range);
                        }
                        else
                        {
                            GreenBalloon greenBalloon = new GreenBalloon(new Point(rect.X, rect.Y), content);
                            List<IActivateable> activatables;
                            TileAggregate trigger;
                            if(pointToTriggerAggregate.TryGetValue(rect.Location, out trigger)
                               && activators.TryGetValue(trigger.properties["Number"], out activatables))
                            {
                                foreach(var a in activatables)
                                    greenBalloon.AddToActivatable(a);
                            }
                            balloon = greenBalloon;
                        }

                        GameManager.getInstance().AddBalloon(balloon);
                    }
                }
            });

            //Remove Non Static Layers
            map.RemoveLayer(triggerLayer);
            map.RemoveLayer(speedLayer);
            map.RemoveLayer(balloonLayer);
            map.RemoveLayer(startLayer);
            map.RemoveLayer(moveablePlatformLayer);
            map.RemoveLayer(zoneLayer);

            unoPos = tempUnoPos;
        }

        delegate void AggregateTileHandler(List<TileAggregate> aggregates, Dictionary<Point, TileAggregate> pointToAggregate);
        private void ParseTiles(xTile.Layers.Layer platformLayer, AggregateTileHandler func)
        {
            List<TileAggregate> aggregates = new List<TileAggregate>();
            Dictionary<Point, TileAggregate> pointToAggregate = new Dictionary<Point, TileAggregate>();

            var width = platformLayer.LayerWidth;
            var height = platformLayer.LayerHeight;
            var tiles = platformLayer.Tiles;
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    if(tiles[x, y] != null)
                    {
                        string type = "";
                        string color = "";
                        Dictionary<string, string> properties = new Dictionary<string, string>();
                        foreach(var property in tiles[x, y].TileSheet.Properties)
                        {
                            switch(property.Key)
                            {
                                case "Type":
                                    type = property.Value;
                                    break;
                                case "Color":
                                    color = property.Value;
                                    break;
                                default:
                                    properties[property.Key] = property.Value;
                                    break;
                            }
                        }

                        TileAggregate aggregate;
                        if(x > 0
                           && pointToAggregate.TryGetValue(new Point(x-1, y), out aggregate)
                           && aggregate.type == type
                           && (aggregate.dir == TileDirection.HORIZONTAL || aggregate.dir == TileDirection.UNDEFINED)
                           && aggregate.color == color)
                        {
                            aggregate.dir = TileDirection.HORIZONTAL;
                            aggregate.rect.Width++;
                            pointToAggregate[new Point(x, y)] = aggregate;
                        }
                        else if(y > 0
                                && pointToAggregate.TryGetValue(new Point(x, y-1), out aggregate)
                                && aggregate.type == type
                                && (aggregate.dir == TileDirection.VERTICAL || aggregate.dir == TileDirection.UNDEFINED)
                                && aggregate.color == color)
                        {
                            aggregate.dir = TileDirection.VERTICAL;
                            aggregate.rect.Height++;
                            pointToAggregate[new Point(x, y)] = aggregate;
                        }
                        else
                        {
                            aggregate = new TileAggregate();
                            aggregate.type = type;
                            aggregate.color = color;
                            aggregate.dir = TileDirection.UNDEFINED;
                            aggregate.rect = new Rectangle(x, y, 1, 1);
                            pointToAggregate[new Point(x, y)] = aggregate;
                            aggregates.Add(aggregate);
                        }

                        foreach(var keyPair in properties)
                        {
                            aggregate.properties[keyPair.Key] = keyPair.Value;
                        }
                    }
                }
            }

            //Combine
            List<TileAggregate> combinedStripsToRemove = new List<TileAggregate>();
            foreach(var aggregate in aggregates)
            {
                TileAggregate aboveAggregate;
                if(aggregate.rect.Y > 0
                   && pointToAggregate.TryGetValue(new Point(aggregate.rect.X, aggregate.rect.Y-1), out aboveAggregate)
                   && aboveAggregate.type == aggregate.type
                   && (aboveAggregate.dir == TileDirection.HORIZONTAL || aboveAggregate.dir == TileDirection.COMBINED)
                   && aboveAggregate.color == aggregate.color
                   && aboveAggregate.rect.X == aggregate.rect.X
                   && aboveAggregate.rect.Width == aggregate.rect.Width)
                {
                    combinedStripsToRemove.Add(aggregate);
                    aboveAggregate.dir = TileDirection.COMBINED;
                    aboveAggregate.rect.Height++;
                    pointToAggregate[new Point(aggregate.rect.X, aggregate.rect.Y)] = aboveAggregate;
                    foreach(var keyPair in aggregate.properties)
                    {
                        aboveAggregate.properties[keyPair.Key] = keyPair.Value;
                    }
                }
            }
            foreach(var aggregate in combinedStripsToRemove)
            {
                aggregates.Remove(aggregate);
            }

            func(aggregates, pointToAggregate);
        }

        float SimUnits(float x)
        {
            return ConvertUnits.ToSimUnits(x);
        }

        Vector2 SimVector(float x, float y)
        {
            return ConvertUnits.ToSimUnits(x, y);
        }

    }
}
