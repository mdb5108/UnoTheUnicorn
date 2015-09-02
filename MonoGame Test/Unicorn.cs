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

using Microsoft.Xna.Framework.Content;
namespace pony
{
    public class Unicorn : DrawableGameComponent
    {
        public Texture2D UnicornTexture;
        public Texture2D []HairTexture;
        private byte hairAmout = 4;    // need to be changed 
        private string[] colorPath = {"blue","green","orange","yellow"};
        private int colorIndex = 0;
        private string colorStatu = "normal";
        public ContentManager Content;

        public Vector2 Position;
        float elapsedtime = 0;
        private bool hitting = false;
        private Game1 _game;
        private bool spacekey = false;
        private bool rightkey = false;
        private bool leftkey = false;
        public string contactbodyname="nothing";
        Body _body;
        public List<string> contactfloornames = new List<string>();
        public List<string> contactcolornames = new List<string>();

        private int JumpForce = 300;

        float JumpX=0, JumpY=0,RestingValueX=0,RestingValueY=0,  // RestingValue: Force needed to get back to resting phase
              RightX=0,RightY=0,RightVeltoCheck=0,RightMaxVel=0, 
              LeftX=0, LeftY = 0,LeftVeltoCheck=0,LeftMaxVel=0;

        private float _resetTime = 0;
        private int Jumps = 0;
        private float JumpelapsedTime = 0;

        float deltaTime = 0;
        private string touchingcolor = "n";
        enum direction
        {
            floor,
            leftwall,
            rightwall,
            ceiling
        };
        direction Direction;


        // n= no color, o=orange,b=blue
        public enum color
        {
            n,
            o,
            b
        }
        public color CurrentColor;

        public Unicorn(Game game) : base(game)
        {
            _game = (Game1)game;
        }

        public Unicorn(Game game, ContentManager Content):base(game)
        {
            this.Content = Content;
            HairTexture = new Texture2D[hairAmout];
        

        }

        public void Initialize(Texture2D texture,Vector2 pos,World world)
        {
            UnicornTexture = texture;
            Position = pos;
            CurrentColor = color.n;
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


            for (int i = 0; i < hairAmout; i++)
            {
                string tempPath = "Uno_" + colorPath[i].ToString();
               // Console.WriteLine(tempPath);
                HairTexture[i] = Content.Load<Texture2D>(tempPath);
            }
          
        }

       
       


        public bool MyOnCollision(Fixture f1, Fixture f2, Contact contact)
        {
            if(f2.Body.BodyName!=null)
            {
                string[] words = f2.Body.BodyName.Split('.');
                // words[0] is the color and words[1] is the side
                // contactbodyname = words[0] + "     " + words[1];
                touchingcolor = words[0];

                if (!contactfloornames.Contains(words[1]))
                {
                    contactfloornames.Add(words[1]);
                }

            }

            return true;
        }

        public void MyOnSeparation(Fixture f1, Fixture f2)
        {
           if(f2.Body.BodyName != null)
            {
                string[] words = f2.Body.BodyName.Split('.');
                touchingcolor = CurrentColor.ToString();
                if (!contactfloornames.Contains(words[1]))
                {
                    contactfloornames.Remove(words[1]);
                   
                }
               
                contactbodyname = touchingcolor;
            }
            JumpelapsedTime = 0;
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
            deltaTime = dt;
            Position = ConvertUnits.ToDisplayUnits(_body.Position.X-0.6f,
                                                    _body.Position.Y-0.7f);
            hitting = contactfloornames.Count==0 ? false : true;
         
            spacekey = Keyboard.GetState().IsKeyDown(Keys.Space);
            rightkey = Keyboard.GetState().IsKeyDown(Keys.Right);
            leftkey = Keyboard.GetState().IsKeyDown(Keys.Left);
           
            ChangeDirections(world,dt);
            CheckColor(dt);
            KeyBoardInput(dt);
            //contactbodyname = _body.LinearVelocity.ToString();
        }

        void CheckColor(float dt)
        {
            // if (CurrentColor.ToString() == "n") return;
            // if(!contactcolornames.Contains(CurrentColor.ToString()))
            if(touchingcolor != CurrentColor.ToString())
            {
                contactbodyname = "hitting bad";
                Bounce(dt);
            }
        }
        void ChangeDirections(World world,float dt)
        {

            if(contactfloornames.Contains("f"))
            {
                Direction = direction.floor;
            }else if(contactfloornames.Contains("l"))
            {
                Direction = direction.leftwall;
            }else if(contactfloornames.Contains("r"))
            {
                Direction = direction.rightwall;
            }else if(contactfloornames.Contains("c"))
            {
                Direction = direction.ceiling;
            }

            switch(Direction)
            {
                case direction.floor:
                    JumpX = 0; JumpY = -1*JumpForce; RestingValueX = 0; RestingValueY = _body.LinearVelocity.Y;
                    RightX = 5; RightY = 0; RightVeltoCheck = _body.LinearVelocity.X; RightMaxVel = 1;
                    LeftX = -5; LeftY = 0; LeftVeltoCheck = _body.LinearVelocity.X; LeftMaxVel = -1;
                    world.Gravity = new Vector2(0, 9.8f);

                   /* _resetTime += dt;
                    if (_resetTime <= dt)
                    {
                        if(_body.LinearVelocity.X > 2)
                        {
                            _body.LinearVelocity = new Vector2(1, _body.LinearVelocity.Y);
                        }
                       
                    }*/
                    break;
                case direction.leftwall:
                    
                    JumpX = JumpForce; JumpY = 0; RestingValueX = _body.LinearVelocity.X; RestingValueY = 0;
                    RightX = 0; RightY = 5; RightVeltoCheck = _body.LinearVelocity.Y; RightMaxVel = 1;
                    LeftX = 0; LeftY = -5; LeftVeltoCheck = _body.LinearVelocity.Y; LeftMaxVel = -1;
                    world.Gravity = new Vector2(-9.8f, 0);
                    break;
                case direction.rightwall:
                    JumpX = -1*JumpForce; JumpY = 0; RestingValueX = _body.LinearVelocity.X; RestingValueY = 0;
                    RightX = 0; RightY = 5; RightVeltoCheck = _body.LinearVelocity.Y; RightMaxVel = 1;
                    LeftX = 0; LeftY = -5; LeftVeltoCheck = _body.LinearVelocity.Y; LeftMaxVel = -1;
                    world.Gravity = new Vector2(9.8f, 0);
                    break;
                case direction.ceiling:
                    JumpX = 0; JumpY = JumpForce; RestingValueX = 0; RestingValueY = _body.LinearVelocity.Y;
                    RightX = 5; RightY = 0; RightVeltoCheck = _body.LinearVelocity.X; RightMaxVel = 1;
                    LeftX = -5; LeftY = 0; LeftVeltoCheck = _body.LinearVelocity.X; LeftMaxVel = -1;
                    world.Gravity = new Vector2(0, -9.8f);
                    break;
                default:
                    break;
            }
        }


        void Bounce(float dt)
        {
            JumpelapsedTime += dt;
            if (JumpelapsedTime <= dt)
            {
                _body.ApplyForce(new Vector2(JumpX, JumpY));
            }
        }
           


        public void ChangeHair(string balloonColor)
        {
            string temp = "Uno_";
            colorStatu = null;
            switch (balloonColor)
            {
                
                case "Balloon_blue":   
                    colorStatu = temp + "blue";
                    colorIndex = 0;
                    break;
                case "Balloon_green":
                    colorStatu = temp + "green";
                    colorIndex = 1;
                    break;
                case "Balloon_orange":
                    colorStatu = temp + "orange";
                    colorIndex = 2;
                    break;
                case "Balloon_red":
                    colorStatu = temp + "blue";
                    break;
                case "Balloon_yellow":
                    colorStatu = temp + "yellow";
                    colorIndex = 3;
                    break;
                default:
                    colorStatu = "normal";
                    break;
            }

        }

        void KeyBoardInput(float dt)
        {
            //////////////////////////////////////////JUMP/////////////////////////////////////////////
            Jumps = hitting ? 0 : Jumps;
            //contactbodyname = Jumps.ToString();
            if (spacekey)
            {
                elapsedtime += dt;
               
               
                if (Jumps<1 && elapsedtime <= dt)
                {
                    Jumps += 1;
                    if (Jumps == 0 && (_body.LinearVelocity.Length()< 0.5f) )
                    {
                       
                        _body.ApplyForce(new Vector2(JumpX, JumpY));
            
                        return;
                    }
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

                if (colorStatu != "normal")
                {
               
                    spritebatch.Draw(HairTexture[colorIndex],Position,null,Color.White,0f,Vector2.Zero,1f,SpriteEffects.None,0f);
                }
            
        }

        double ConvertDegreetoRadians(float degree)
        {
            double radian = 0;

            radian = degree * (Math.PI/ 180);

            return radian;

        }

    }
}
