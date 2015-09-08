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

        public List<Walls> walls = new List<Walls>();

        private static readonly float TILE_SIZE = 32f;
        private static readonly float TILE_SIZE_CONV = ConvertUnits.ToSimUnits(TILE_SIZE);

        private Unicorn Uno;

        private enum TileDirection {COMBINED, HORIZONTAL, VERTICAL, UNDEFINED};
        class TileAggregate
        {
            public string type;
            public string color;
            public TileDirection dir;
            public Rectangle rect;
        }

        public void Initialize(int level,World world, ContentManager content, out Map map)
        {
            var stream = TitleContainer.OpenStream("Content\\Map"+level+".tbin");
            map = xTile.Format.FormatManager.Instance.BinaryFormat.Load(stream);

            ParseMap(ref map, world, content);
        }

        private void ParseMap(ref Map map, World world, ContentManager content)
        {
            var layerCount = map.Layers.Count;

            xTile.Layers.Layer platformLayer = map.Layers[layerCount-3];
            xTile.Layers.Layer zoneLayer = map.Layers[layerCount-2];
            xTile.Layers.Layer balloonLayer = map.Layers[layerCount-1];

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

            Dictionary<Point, TileAggregate> pointToZoneAggregate = null;

            ParseTiles(zoneLayer, delegate(List<TileAggregate> aggregates, Dictionary<Point, TileAggregate> pointToAggregate)
            {
                pointToZoneAggregate = pointToAggregate;
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

                        GameManager.getInstance().AddBalloon(new Balloon(new Point(rect.X, rect.Y), content, aggregate.color, speed, range));
                    }
                }
            });

            //Remove Non Static Layers
            map.RemoveLayer(balloonLayer);
            map.RemoveLayer(zoneLayer);
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
                }
            }
            foreach(var aggregate in combinedStripsToRemove)
            {
                aggregates.Remove(aggregate);
            }

            func(aggregates, pointToAggregate);
        }

        public void InitializeBoundaries(World world)
        {
            // n=no color, n = non magnetic wall
            _floor = new Walls("", world, 48, 1, new Point(0, 31), true);
            _rightwall = new Walls("", world, 1, 32, new Point(47, 0), true);
            _leftwall = new Walls("", world, 1, 32, new Point(0, 0), true);
            _ceiling = new Walls("", world, 48, 1, new Point(0, 0), true);
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
