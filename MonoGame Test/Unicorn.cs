using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using MonoGame_Test;
namespace pony
{
    class Unicorn : DrawableGameComponent
    {
        public Texture2D UnicornTexture;

        public Vector2 Position;
        float elapsedtime = 0;
        private bool hitting = false;
        private Game1 _game;
        private bool spacekey = false;
        private bool rightkey = false;
        private bool leftkey = false;
        public string contactbodyname="nothing";
        Body _body;
        public List<string> contactnames=new List<string>();

        float JumpX=0, JumpY=0,RestingValueX=0,RestingValueY=0,  // RestingValue: Force needed to get back to resting phase
              RightX=0,RightY=0,RightVeltoCheck=0,RightMaxVel=0, 
              LeftX=0, LeftY = 0,LeftVeltoCheck=0,LeftMaxVel=0;

        private float _resetTime = 0;

        enum direction
        {
            floor,
            leftwall,
            rightwall,
            ceiling
        };
        direction Direction;

        public Unicorn(Game game):base(game)
        {
            _game = (Game1)game;
        }

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
            _body.BodyName = "Unicorn";
             hitting = false;
            _body.OnCollision += MyOnCollision;
            _body.OnSeparation += MyOnSeparation;
        }

        public bool MyOnCollision(Fixture f1, Fixture f2, Contact contact)
        {
            if(f2.Body.BodyName == "baloon")
            {
              

            }
            if(!contactnames.Contains(f2.Body.BodyName))
            {
                contactnames.Add(f2.Body.BodyName);
            }
 
            return true;
        }

        public void MyOnSeparation(Fixture f1, Fixture f2)
        {
            if(contactnames.Contains(f2.Body.BodyName))
            {
                contactnames.Remove(f2.Body.BodyName);
                _resetTime = 0;
            }
            contactbodyname = "  " ;
        }

        protected override void UnloadContent()
        {
            _body.OnCollision -= MyOnCollision;
            _body.OnSeparation -= MyOnSeparation;
            UnicornTexture.Dispose();
        }
        // dt is deltatime
        public void Update(GameTime gametime,float dt,World world)
        {
            Position = ConvertUnits.ToDisplayUnits(_body.Position.X-0.6f,
                                                    _body.Position.Y-0.7f);
            hitting = contactnames.Count==0 ? false : true;
           // _body.Rotation = 90*dt;
            spacekey = Keyboard.GetState().IsKeyDown(Keys.Space);
            rightkey = Keyboard.GetState().IsKeyDown(Keys.Right);
            leftkey = Keyboard.GetState().IsKeyDown(Keys.Left);

            ChangeDirections(world,dt);
            KeyBoardInput(dt);
            contactbodyname = _body.LinearVelocity.ToString();
        }

        void ChangeDirections(World world,float dt)
        {

            if(contactnames.Contains("floor"))
            {
                Direction = direction.floor;
            }else if(contactnames.Contains("left"))
            {
                Direction = direction.leftwall;
            }else if(contactnames.Contains("right"))
            {
                Direction = direction.rightwall;
            }else if(contactnames.Contains("ceiling"))
            {
                Direction = direction.ceiling;
            }

            switch(Direction)
            {
                case direction.floor:
                    

                    JumpX = 0; JumpY = -400; RestingValueX = 0; RestingValueY = _body.LinearVelocity.Y;
                    RightX = 5; RightY = 0; RightVeltoCheck = _body.LinearVelocity.X; RightMaxVel = 1;
                    LeftX = -5; LeftY = 0; LeftVeltoCheck = _body.LinearVelocity.X; LeftMaxVel = -1;
                    world.Gravity = new Vector2(0, 9.8f);

                    _resetTime += dt;
                    if (_resetTime <= dt)
                    {
                        if(_body.LinearVelocity.X > 2)
                        {
                            _body.LinearVelocity = new Vector2(1, _body.LinearVelocity.Y);
                        }
                       
                    }
                    break;
                case direction.leftwall:
                    
                    JumpX = 400; JumpY = 0; RestingValueX = _body.LinearVelocity.X; RestingValueY = 0;
                    RightX = 0; RightY = 5; RightVeltoCheck = _body.LinearVelocity.Y; RightMaxVel = 1;
                    LeftX = 0; LeftY = -5; LeftVeltoCheck = _body.LinearVelocity.Y; LeftMaxVel = -1;
                    world.Gravity = new Vector2(-9.8f, 0);
                    break;
                case direction.rightwall:
                    JumpX = -400; JumpY = 0; RestingValueX = _body.LinearVelocity.X; RestingValueY = 0;
                    RightX = 0; RightY = 5; RightVeltoCheck = _body.LinearVelocity.Y; RightMaxVel = 1;
                    LeftX = 0; LeftY = -5; LeftVeltoCheck = _body.LinearVelocity.Y; LeftMaxVel = -1;
                    world.Gravity = new Vector2(9.8f, 0);
                    break;
                case direction.ceiling:
                    JumpX = 0; JumpY = 400; RestingValueX = 0; RestingValueY = _body.LinearVelocity.Y;
                    RightX = 5; RightY = 0; RightVeltoCheck = _body.LinearVelocity.X; RightMaxVel = 1;
                    LeftX = -5; LeftY = 0; LeftVeltoCheck = _body.LinearVelocity.X; LeftMaxVel = -1;
                    world.Gravity = new Vector2(0, -9.8f);
                    break;
                default:
                    break;
            }
        }

        void KeyBoardInput(float dt)
        {
            //////////////////////////////////////////JUMP/////////////////////////////////////////////
            if (spacekey)
            {
                elapsedtime += dt;
                if (hitting && elapsedtime <= dt)
                {
                   
                   _body.ApplyForce(new Vector2(JumpX, JumpY));
                    
                }
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.Space))
            {
                elapsedtime = 0;
            }
            /////////////////////////////////////////RIGHT///////////////////////////////////////////////
            if (rightkey)
            {
                if (RightVeltoCheck < RightMaxVel || RightVeltoCheck==0)
                {
                    _body.ApplyForce(new Vector2(RightX, RightY));

                }
            }
            else if (!leftkey && Keyboard.GetState().IsKeyUp(Keys.Right))
            {
                _body.LinearVelocity = new Vector2(RestingValueX, RestingValueY);
            }
            ///////////////////////////////////////////LEFT///////////////////////////////////////////////
            if (leftkey)
            {
                if (LeftVeltoCheck > LeftMaxVel || LeftVeltoCheck == 0)
                {
                    _body.ApplyForce(new Vector2(LeftX, LeftY));
                }
              
            }
            else if (!rightkey && Keyboard.GetState().IsKeyUp(Keys.Left))
            {
                _body.LinearVelocity = new Vector2(RestingValueX,RestingValueY);
            }
        }

        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Draw(UnicornTexture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        double ConvertDegreetoRadians(float degree)
        {
            double radian = 0;

            radian = degree * (Math.PI/ 180);

            return radian;

        }

    }
}
