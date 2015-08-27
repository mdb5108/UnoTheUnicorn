using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace pony
{
    class Unicorn
    {
        public Texture2D UnicornTexture;

        public Vector2 Position;
        float elapsedtime = 0;
        private bool hitting = false;

        private bool spacekey = false;
        private bool rightkey = false;

        Body _body;
       

        public void Initialize(Texture2D texture,Vector2 pos,World world)
        {
            UnicornTexture = texture;
            Position = pos;

            _body = BodyFactory.CreateRectangle(world,
                                                ConvertUnits.ToSimUnits(80),
                                                ConvertUnits.ToSimUnits(80), 0f);
            _body.BodyType = BodyType.Dynamic;
            _body.Restitution = 0f;
            _body.Position = ConvertUnits.ToSimUnits(pos.X,pos.Y);
            hitting = false;
        }

        // dt is deltatime
        public void Update(GameTime gametime,float dt)
        {
            Position = ConvertUnits.ToDisplayUnits(_body.Position.X-0.5f,
                                                    _body.Position.Y-0.7f);
          //  hitting = Position.Y > 350 ? true : false;

            spacekey = Keyboard.GetState().IsKeyDown(Keys.Space);
            rightkey = Keyboard.GetState().IsKeyDown(Keys.Right);
            if (spacekey)
            {
                elapsedtime += dt;
                if(elapsedtime<=dt)
                {
                    if (_body.LinearVelocity.Y > -6)
                    {
                        _body.ApplyForce(new Vector2(0, -400), new Vector2(0, 2));
                    }
                }  
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.Space))
            {
                elapsedtime = 0;
            }


            if (rightkey)
            {
               if(_body.LinearVelocity.X < 1)
                {
                    _body.ApplyForce(new Vector2(5, 0));
                }
            }else if(Keyboard.GetState().IsKeyUp(Keys.Right))
            {
                _body.LinearVelocity = new Vector2(0, _body.LinearVelocity.Y);
            }

        }

        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Draw(UnicornTexture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }


    }
}
