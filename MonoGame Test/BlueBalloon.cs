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
using Microsoft.Xna.Framework.Content;

using Game2;
using pony;

namespace MonoGame_Test
{
    class BlueBalloon : Balloon
    {
        static readonly float SCARE_RADIUS = ConvertUnits.ToSimUnits(2*GameManager.UnoToTiles * GameManager.TILE_SIZE);
        static readonly float ACCELERATION_MODIFIER = 3f;

        public BlueBalloon(Point position, World world, ContentManager content, float speed)
            : base(position, content, "b", speed, 0)
        {
            float tileSize = GameManager.TILE_SIZE;
            _body = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(width), ConvertUnits.ToSimUnits(height), 0f);
            _body.BodyType = BodyType.Dynamic;
            _body.Restitution = .5f;
            _body.Friction = 0f;
            _body.Position = ConvertUnits.ToSimUnits(position.X * tileSize, position.Y * tileSize);
            _body.GravityScale = 0;
            _body.OnCollision += MyOnCollision;
            
        }

        public bool MyOnCollision(Fixture f1, Fixture f2, Contact contact)
        {
            return f2.Body.BodyName==null;
        }

        public override Balloon Update(Unicorn unicorn)
        {
            Balloon balloon = base.Update(unicorn);
            Vector2 difference = _body.Position - ConvertUnits.ToSimUnits(unicorn.CenterPosition);
            if (isActive && difference.Length() <= SCARE_RADIUS)
            {
                _body.ApplyForce(Vector2.Normalize(difference)*ACCELERATION_MODIFIER*9.8f);
            }
            else
            {
                _body.LinearVelocity = Vector2.Zero;
            }
            Position = ConvertUnits.ToDisplayUnits(_body.Position) - new Vector2(width / 2, height / 2);
            return balloon;
        }
    }
}
