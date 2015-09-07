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
using FarseerPhysics.Common;
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
        public string contactfloorname = "f";
        public List<string> contactcolornames = new List<string>();

        private Dictionary<string, HashSet<Rectangle>> auraContacts;

        private int JumpForce = 220;
        private float runForce = 200;

        float JumpX=0, JumpY=0,RestingValueX=0,RestingValueY=0,  // RestingValue: Force needed to get back to resting phase
              RightX=0,RightY=0,RightVeltoCheck=0,RightMaxVel=0, 
              LeftX=0, LeftY = 0,LeftVeltoCheck=0,LeftMaxVel=0;

        private float _resetTime = 0;
        private int Jumps = 0;

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
            b,
            g,
            o,
            r,
            y,
        }
        public color CurrentColor;

        private bool bouncing = false;
        private Vector2 bouncingForce;

        private float width = 128;
        private float height = 128;

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
            auraContacts = new Dictionary<string, HashSet<Rectangle>>()
            {
                {"o", new HashSet<Rectangle>()},
                {"b", new HashSet<Rectangle>()},
                {"g", new HashSet<Rectangle>()},
                {"y", new HashSet<Rectangle>()},
            };

            UnicornTexture = texture;
            Position = pos;
            CurrentColor = color.n;
            _body = BodyFactory.CreateRectangle(world,
                                                ConvertUnits.ToSimUnits(96),
                                                ConvertUnits.ToSimUnits(96), 0f);
            _body.BodyType = BodyType.Dynamic;
            _body.Restitution = 0f;
            _body.Friction = 0f;
            _body.Position = ConvertUnits.ToSimUnits(pos.X,pos.Y);
            _body.BodyName = "Unicorn";
            _body.UserData = new Vector2(96, 96);
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

                Vector2 size = (Vector2)f2.Body.UserData;
                Vector2 pos = ConvertUnits.ToDisplayUnits(f2.Body.Position);
                if(CurrentColor == color.n)
                {
                    auraContacts[touchingcolor].Add(new Rectangle((int)(pos.X-(size.X/2)), (int)(pos.Y-(size.Y/2)), (int)size.X, (int)size.Y));
                    return false;
                }
                else if(touchingcolor == CurrentColor.ToString())
                {
                    auraContacts[touchingcolor].Add(new Rectangle((int)(pos.X-(size.X/2)), (int)(pos.Y-(size.Y/2)), (int)size.X, (int)size.Y));
                    contactfloorname = words[1];
                    return false;
                }
                else
                {
                    Vector2 jumpingForce = new Vector2();
                    switch(words[1])
                    {
                        case "f":
                            jumpingForce.Y = -JumpForce;
                            break;
                        case "c":
                            jumpingForce.Y = JumpForce;
                            break;
                        case "l":
                            jumpingForce.X = JumpForce;
                            break;
                        case "r":
                            jumpingForce.X = -JumpForce;
                            break;
                    }
                    bouncing = true;
                    bouncingForce = jumpingForce;
                }
            }

            Vector2 normal;
            FixedArray2<Vector2> points;
            contact.GetWorldManifold(out normal, out points);
            switch(Direction)
            {
                case direction.floor:
                    if(normal.Y < 0)
                        hitting = true;
                    break;
                case direction.ceiling:
                    if(normal.Y > 0)
                        hitting = true;
                    break;
                case direction.leftwall:
                    if(normal.X < 0)
                        hitting = true;
                    break;
                case direction.rightwall:
                    if(normal.Y > 0)
                        hitting = true;
                    break;

            }
            return true;
        }

        public void MyOnSeparation(Fixture f1, Fixture f2)
        {
           if(f2.Body.BodyName != null)
            {
                string[] words = f2.Body.BodyName.Split('.');
                touchingcolor = CurrentColor.ToString();
                Vector2 size = (Vector2)f2.Body.UserData;
                Vector2 pos = ConvertUnits.ToDisplayUnits(f2.Body.Position);
                auraContacts[words[0]].Remove(new Rectangle((int)(pos.X-(size.X/2)), (int)(pos.Y-(size.Y/2)), (int)size.X, (int)size.Y));
                contactfloorname = "";
               
                contactbodyname = touchingcolor;
            }
           hitting = false;
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

            CheckTriggers();

            //If we are not in field, fall down
            if(touchingcolor != "" && touchingcolor != "n")
            {
                if(auraContacts[touchingcolor].Count == 0)
                {
                    contactfloorname = "f";
                }
            }
         
            spacekey = Keyboard.GetState().IsKeyDown(Keys.Space);
            rightkey = Keyboard.GetState().IsKeyDown(Keys.Right);
            leftkey = Keyboard.GetState().IsKeyDown(Keys.Left);
           
            ChangeDirections(world,dt);
            CheckColor(dt);
            KeyBoardInput(dt);
            //contactbodyname = _body.LinearVelocity.ToString();
        }

        void CheckTriggers()
        {
            Vector2 size = (Vector2)_body.UserData;
            Vector2 pos = ConvertUnits.ToDisplayUnits(_body.Position);
            Rectangle bounds = new Rectangle((int)(pos.X-(size.X/2)), (int)(pos.Y-(size.Y/2)), (int)size.X, (int)size.Y);
            List<KeyValuePair<string, Rectangle>> removed = new List<KeyValuePair<string, Rectangle>>();
            foreach(var keyRects in auraContacts)
            {
                foreach(Rectangle r in keyRects.Value)
                {
                    if(!bounds.Intersects(r))
                    {
                        removed.Add(new KeyValuePair<string, Rectangle>(keyRects.Key, r));
                    }
                }
            }

            foreach(var keyRect in removed)
            {
                auraContacts[keyRect.Key].Remove(keyRect.Value);
            }
        }

        void CheckColor(float dt)
        {
            // if (CurrentColor.ToString() == "n") return;
            // if(!contactcolornames.Contains(CurrentColor.ToString()))
            if(CurrentColor != color.n && touchingcolor != CurrentColor.ToString())
            {
                contactbodyname = "hitting bad";
                Bounce(dt);
            }
        }
        void ChangeDirections(World world,float dt)
        {

            if(contactfloorname == "f")
            {
                Direction = direction.floor;
            }else if(contactfloorname == "l")
            {
                Direction = direction.leftwall;
            }else if(contactfloorname == "r")
            {
                Direction = direction.rightwall;
            }else if(contactfloorname == "c")
            {
                Direction = direction.ceiling;
            }

            switch(Direction)
            {
                case direction.floor:
                    JumpX = 0; JumpY = -1*JumpForce; RestingValueX = 0; RestingValueY = _body.LinearVelocity.Y;
                    RightX = runForce; RightY = 0; RightVeltoCheck = _body.LinearVelocity.X; RightMaxVel = 1;
                    LeftX = -runForce; LeftY = 0; LeftVeltoCheck = _body.LinearVelocity.X; LeftMaxVel = -1;
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
                    RightX = 0; RightY = runForce; RightVeltoCheck = _body.LinearVelocity.Y; RightMaxVel = 1;
                    LeftX = 0; LeftY = -runForce; LeftVeltoCheck = _body.LinearVelocity.Y; LeftMaxVel = -1;
                    world.Gravity = new Vector2(-9.8f, 0);
                    break;
                case direction.rightwall:
                    JumpX = -1*JumpForce; JumpY = 0; RestingValueX = _body.LinearVelocity.X; RestingValueY = 0;
                    RightX = 0; RightY = runForce; RightVeltoCheck = _body.LinearVelocity.Y; RightMaxVel = 1;
                    LeftX = 0; LeftY = -runForce; LeftVeltoCheck = _body.LinearVelocity.Y; LeftMaxVel = -1;
                    world.Gravity = new Vector2(9.8f, 0);
                    break;
                case direction.ceiling:
                    JumpX = 0; JumpY = JumpForce; RestingValueX = 0; RestingValueY = _body.LinearVelocity.Y;
                    RightX = runForce; RightY = 0; RightVeltoCheck = _body.LinearVelocity.X; RightMaxVel = 1;
                    LeftX = -runForce; LeftY = 0; LeftVeltoCheck = _body.LinearVelocity.X; LeftMaxVel = -1;
                    world.Gravity = new Vector2(0, -9.8f);
                    break;
                default:
                    break;
            }
        }

        void Bounce(float dt)
        {
            if (bouncing)
            {
                _body.ApplyForce(bouncingForce);
                bouncing = false;
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
                    CurrentColor = color.b;
                    break;
                case "Balloon_green":
                    colorStatu = temp + "green";
                    colorIndex = 1;
                    CurrentColor = color.g;
                    break;
                case "Balloon_orange":
                    colorStatu = temp + "orange";
                    colorIndex = 2;
                    CurrentColor = color.o;
                    break;
                case "Balloon_red":
                    colorStatu = temp + "red";
                    CurrentColor = color.r;
                    break;
                case "Balloon_yellow":
                    colorStatu = temp + "yellow";
                    colorIndex = 3;
                    CurrentColor = color.y;
                    break;
                default:
                    colorStatu = "normal";
                    CurrentColor = color.n;
                    break;
            }

            if(touchingcolor != CurrentColor.ToString())
            {
                contactfloorname = "f";
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

        public Vector2 GetCenterPos()
        {
            return Position + new Vector2(width/2, height/2);
        }

        public void Draw(SpriteBatch spritebatch)
        {
                float rotation = 0f;
                SpriteEffects imageDirection;

                switch(Direction)
                {
                    case  direction.floor:
                    default:
                        rotation = 0f;
                        if(RightVeltoCheck > 0f)
                            imageDirection = SpriteEffects.FlipHorizontally;
                        else
                            imageDirection = SpriteEffects.None;
                        break;
                    case direction.leftwall:
                        rotation = (float)(.5f * Math.PI);
                        if(RightVeltoCheck > 0f)
                            imageDirection = SpriteEffects.FlipHorizontally;
                        else
                            imageDirection = SpriteEffects.None;
                        break;
                    case direction.rightwall:
                        rotation = (float)(1.5f * Math.PI);
                        if(RightVeltoCheck > 0f)
                            imageDirection = SpriteEffects.None;
                        else
                            imageDirection = SpriteEffects.FlipHorizontally;
                        break;
                    case direction.ceiling:
                        rotation = (float)Math.PI;
                        if(RightVeltoCheck > 0f)
                            imageDirection = SpriteEffects.None;
                        else
                            imageDirection = SpriteEffects.FlipHorizontally;
                        break;
                }
                Vector2 relativeCenter = new Vector2(width/2, height/2);
                spritebatch.Draw(UnicornTexture, Position+relativeCenter, null, Color.White, rotation, relativeCenter, 1f, imageDirection, 0f);
                if (colorStatu != "normal")
                {
                    spritebatch.Draw(HairTexture[colorIndex],Position+relativeCenter,null,Color.White,rotation,relativeCenter,1f,imageDirection,0f);
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
