using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.DebugView;
using Microsoft.Xna.Framework;
using MonoGame_Test;
using pony;

namespace levels
{
    class Levels
    {

        public Walls _floor;            // body that is effected by physics
        public Walls _rightwall;
        public Walls _leftwall;
        public Walls _ceiling;

        private Unicorn Uno;
     
        public void Initialize(int level,World world)
        {
            switch(level)
            {
                case 1:
                    Walls slab = new Walls("n.n", world, ConvertUnits.ToSimUnits(200),
                                                         ConvertUnits.ToSimUnits(10), 10f,
                                                         ConvertUnits.ToSimUnits(300, 500), 
                                                         true);
                    Walls slab1 = new Walls("n.n", world, ConvertUnits.ToSimUnits(100),
                                                           ConvertUnits.ToSimUnits(10), 10f,
                                                           ConvertUnits.ToSimUnits(600, 200),
                                                           true);

                   _floor.CreateSensor(0, "o.f", world, SimUnits(100),
                                                      SimUnits(50),
                                                      SimVector(300, Game1.Height-50),1.0f);

                    break;
                default:
                    break;
            }
            

        }

        public void InitializeBoundaries(World world)
        {
            // n=no color, n = non magnetic wall
            _floor = new Walls(world, ConvertUnits.ToSimUnits(Game1.Width),
                                              ConvertUnits.ToSimUnits(10), 10f, 
                                              ConvertUnits.ToSimUnits(Game1.Width / 2, Game1.Height - 10),
                                              true);

            _rightwall = new Walls(world, ConvertUnits.ToSimUnits(10),
                                              ConvertUnits.ToSimUnits(Game1.Height), 10f,
                                              ConvertUnits.ToSimUnits(Game1.Width - 10, Game1.Height / 2),
                                              true);
            _leftwall = new Walls(world, ConvertUnits.ToSimUnits(10),
                                              ConvertUnits.ToSimUnits(Game1.Height), 10f,
                                              ConvertUnits.ToSimUnits(10, Game1.Height / 2),
                                              true);
            _ceiling = new Walls(world, ConvertUnits.ToSimUnits(Game1.Width),
                                              ConvertUnits.ToSimUnits(10), 10f,
                                              ConvertUnits.ToSimUnits(Game1.Width / 2, 10),
                                              true);
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
