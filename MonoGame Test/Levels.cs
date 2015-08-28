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



                    break;
                default:
                    break;
            }
            

        }

        public void InitializeBoundaries(World world)
        {
            _floor = new Walls("floor",world, ConvertUnits.ToSimUnits(Game1.Width),
                                              ConvertUnits.ToSimUnits(10), 10f, 
                                              ConvertUnits.ToSimUnits(Game1.Width / 2, Game1.Height - 10),
                                              true);

            _rightwall = new Walls("right", world, ConvertUnits.ToSimUnits(10),
                                              ConvertUnits.ToSimUnits(Game1.Height), 10f,
                                              ConvertUnits.ToSimUnits(Game1.Width - 10, Game1.Height / 2),
                                              true);
            _leftwall = new Walls("left", world, ConvertUnits.ToSimUnits(10),
                                              ConvertUnits.ToSimUnits(Game1.Height), 10f,
                                              ConvertUnits.ToSimUnits(10, Game1.Height / 2),
                                              true);
            _ceiling = new Walls("ceiling", world, ConvertUnits.ToSimUnits(Game1.Width),
                                              ConvertUnits.ToSimUnits(10), 10f,
                                              ConvertUnits.ToSimUnits(Game1.Width / 2, 10),
                                              true);
        }

    }
}
