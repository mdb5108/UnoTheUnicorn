using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.DebugView;
using MonoGame_Test;

namespace levels
{
    class Levels
    {

        Body _floor;            // body that is effected by physics
        Body _rightwall;
        Body _leftwall;
        Body _ceiling;


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
            _floor = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(Game1.Width),
                                              ConvertUnits.ToSimUnits(10), 10f);

            _rightwall = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(10),
                                                           ConvertUnits.ToSimUnits(Game1.Height), 10f);
            _leftwall = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(10),
                                                           ConvertUnits.ToSimUnits(Game1.Height), 10f);
            _ceiling = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(Game1.Width),
                                              ConvertUnits.ToSimUnits(10), 10f);

            _floor.Position = ConvertUnits.ToSimUnits(Game1.Width/2 , Game1.Height-10);
            _rightwall.Position = ConvertUnits.ToSimUnits(Game1.Width - 10, Game1.Height/2);
            _leftwall.Position = ConvertUnits.ToSimUnits(10, Game1.Height / 2);
            _ceiling.Position = ConvertUnits.ToSimUnits(Game1.Width / 2, 10);

            _floor.IsStatic = true;
            _rightwall.IsStatic = true;
            _leftwall.IsStatic = true;
            _ceiling.IsStatic = true;

            _floor.Restitution = 0f;
            _rightwall.Restitution = 0f;
            _leftwall.Restitution = 0f;
            _ceiling.Restitution = 0f;

            _floor.Friction = 0;
            _rightwall.Friction = 0;
            _leftwall.Friction = 0;
            _ceiling.Friction = 0;
        }

    }
}
