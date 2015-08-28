using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.DebugView;
using Microsoft.Xna.Framework;

namespace MonoGame_Test
{
    class Walls
    {

        public Body _body;            // body that is effected by physics
        public string Name;

        public Walls(string name, World world, float width, float height, float density, Vector2 pos,bool Static)
        {
            Name = name;
            _body = BodyFactory.CreateRectangle(world, width,height, density);
            _body.Position = pos;
            _body.IsStatic = Static;
            _body.Restitution = 0;
            _body.Friction = 0;
            _body.OnCollision += MyOnCollision;
            _body.BodyName = name;
           
        }

        ~Walls()
        {
            _body.OnCollision -= MyOnCollision;
        }

        public bool MyOnCollision(Fixture f1, Fixture f2, Contact contact)
        {

           


            return true;
        }


    }
}
