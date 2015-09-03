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

        public void Initialize(int level,World world, ContentManager content)
        {
            switch(level)
            {
                case 1:
                    //Top Left orange
                    walls.Add(new Walls("o", world, 5, 1, new Point(0, 3), true));
                    walls.Add(new Walls("o", world, 1, 11, new Point(0, 3), true));
                    walls.Add(new Walls("o", world, 23, 1, new Point(0, 13), true));

                    //Top left yellow
                    walls.Add(new Walls("y", world, 7, 1, new Point(7, 8), true));
                    walls.Add(new Walls("y", world, 5, 1, new Point(23, 8), true));

                    //Top Right Normal
                    walls.Add(new Walls("", world, 1, 7, new Point(33, 3), true));
                    walls.Add(new Walls("", world, 8, 1, new Point(33, 3), true));
                    walls.Add(new Walls("", world, 1, 7, new Point(40, 3), true));

                    //Bottom Left Normal
                    walls.Add(new Walls("", world, 8, 1, new Point(8, 25), true));
                    walls.Add(new Walls("", world, 1, 3, new Point(15, 23), true));
                    walls.Add(new Walls("", world, 4, 1, new Point(15, 23), true));

                    //Bottom Right Normal
                    walls.Add(new Walls("", world, 1, 6, new Point(26, 18), true));
                    walls.Add(new Walls("", world, 8, 1, new Point(26, 18), true));

                    //Bottom Right Orange
                    walls.Add(new Walls("o", world, 6, 1, new Point(34, 18), true));

                    //Bottom Right Yellow
                    walls.Add(new Walls("y", world, 9, 1, new Point(34, 26), true));

                    //Initialize the balloons
                    GameManager.getInstance().AddBalloon(new Balloon(new Point(10, 1), content, "y"));
                    GameManager.getInstance().AddBalloon(new Balloon(new Point(36, 5), content, "o"));
                    GameManager.getInstance().AddBalloon(new Balloon(new Point(12, 18), content, "o"));
                    GameManager.getInstance().AddBalloon(new Balloon(new Point(33, 19), content, "y"));

                    break;
                default:
                    break;
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
