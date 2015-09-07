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

        private enum WallDirection {COMBINED, HORIZONTAL, VERTICAL, UNDEFINED};
        class WallAggregate
        {
            public string color;
            public WallDirection dir;
            public Rectangle rect;
        }

        public void Initialize(int level,World world, ContentManager content, out Map map)
        {
            var stream = TitleContainer.OpenStream("Content\\Map"+level+".tbin");
            map = xTile.Format.FormatManager.Instance.BinaryFormat.Load(stream);

            ParseMap(ref map, world);

            switch(level)
            {
                case 1:
                    {
                      //Initialize the balloons
                      GameManager.getInstance().AddBalloon(new Balloon(new Point(10, 1), content, "y",1f,300f));
                      GameManager.getInstance().AddBalloon(new Balloon(new Point(36, 5), content, "o",0f,0f));
                      GameManager.getInstance().AddBalloon(new Balloon(new Point(12, 18), content, "o",0f,0f));
                      GameManager.getInstance().AddBalloon(new Balloon(new Point(33, 19), content, "y",2f,100f));
                    }
                    break;
                default:
                    break;
            }
            

        }

        private void ParseMap(ref Map map, World world)
        {
            var layerCount = map.Layers.Count;
            xTile.Layers.Layer platformLayer = map.Layers[layerCount-1];

            List<WallAggregate> aggregates = new List<WallAggregate>();
            Dictionary<Point, WallAggregate> pointToAggregate = new Dictionary<Point, WallAggregate>();

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

                        if(type == "Wall")
                        {
                            color = "";
                        }

                        if(type == "Wall" || type == "Magnetic Wall")
                        {
                            WallAggregate aggregate;
                            if(x > 0
                               && pointToAggregate.TryGetValue(new Point(x-1, y), out aggregate)
                               && (aggregate.dir == WallDirection.HORIZONTAL || aggregate.dir == WallDirection.UNDEFINED)
                               && aggregate.color == color)
                            {
                                aggregate.dir = WallDirection.HORIZONTAL;
                                aggregate.rect.Width++;
                                pointToAggregate[new Point(x, y)] = aggregate;
                            }
                            else if(y > 0
                                    && pointToAggregate.TryGetValue(new Point(x, y-1), out aggregate)
                                    && (aggregate.dir == WallDirection.VERTICAL || aggregate.dir == WallDirection.UNDEFINED)
                                    && aggregate.color == color)
                            {
                                aggregate.dir = WallDirection.VERTICAL;
                                aggregate.rect.Height++;
                                pointToAggregate[new Point(x, y)] = aggregate;
                            }
                            else
                            {
                                aggregate = new WallAggregate();
                                aggregate.color = color;
                                aggregate.dir = WallDirection.UNDEFINED;
                                aggregate.rect = new Rectangle(x, y, 1, 1);
                                pointToAggregate[new Point(x, y)] = aggregate;
                                aggregates.Add(aggregate);
                            }
                        }
                    }
                }
            }

            //Combine
            List<WallAggregate> combinedStripsToRemove = new List<WallAggregate>();
            foreach(var aggregate in aggregates)
            {
                WallAggregate aboveAggregate;
                if(aggregate.rect.Y > 0
                   && pointToAggregate.TryGetValue(new Point(aggregate.rect.X, aggregate.rect.Y-1), out aboveAggregate)
                   && (aboveAggregate.dir == WallDirection.HORIZONTAL || aboveAggregate.dir == WallDirection.COMBINED)
                   && aboveAggregate.color == aggregate.color
                   && aboveAggregate.rect.X == aggregate.rect.X
                   && aboveAggregate.rect.Width == aggregate.rect.Width)
                {
                    combinedStripsToRemove.Add(aggregate);
                    aboveAggregate.dir = WallDirection.COMBINED;
                    aboveAggregate.rect.Height++;
                    pointToAggregate[new Point(aggregate.rect.X, aggregate.rect.Y)] = aboveAggregate;
                }
            }
            foreach(var aggregate in combinedStripsToRemove)
            {
                aggregates.Remove(aggregate);
            }

            foreach(var aggregate in aggregates)
            {
                Rectangle rect = aggregate.rect;
                walls.Add(new Walls(aggregate.color, world, (uint)rect.Width, (uint)rect.Height, new Point(rect.X, rect.Y), true));
            }
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
