using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using pony;
using Game2;
namespace MonoGame_Test
{
    public class Balloon
    {
        private Texture2D Texture;
        public string path;
        protected Vector2 Position;
        private Vector2 startPosition;
        public bool isActive;

        private Texture2D popTexture;

        protected float width = 64;
        protected float height = 64;
       

        public ContentManager Content;
        protected Body _body;
        public string name;


        protected float speed = 0f;
        float range = 0f;

        public Balloon(Point position, ContentManager Content, string color, float speed, int range)
        {
            var tileSize = GameManager.TILE_SIZE;
            Vector2 Position = new Vector2(position.X*tileSize, position.Y*tileSize);
            startPosition = Position;
            string colorPath = "Balloon_";
            switch(color)
            {
                case "o":
                default:
                    colorPath += "orange";
                    break;
                case "b":
                    colorPath += "blue";
                    break;
                case "g":
                    colorPath += "green";
                    break;
                case "r":
                    colorPath += "red";
                    break;
                case "y":
                    colorPath += "yellow";
                    this.range = range*tileSize;
                    break;
            }
            this.speed = speed;
            InitializeBase(Position, Content, colorPath);
        }

        

        public Balloon(Vector2 Position, ContentManager Content, string colorPath)
        {
            InitializeBase(Position, Content, colorPath);
        }
        public Balloon(World world,Vector2 pos, ContentManager Content)
        {
            _body = BodyFactory.CreateCircle(world, 0.35f, 2.0f);
            _body.Position = ConvertUnits.ToSimUnits(pos.X+55,pos.Y+45);
            _body.BodyName = "baloon";
            
            this.Position = pos;
            this.Content = Content;
        }

        private void InitializeBase(Vector2 Position, ContentManager Content, string colorPath)
        {
            this.Position = Position;
            this.Content = Content;
            isActive = true;
            path = colorPath;
        }

        public void Destroy()
        {
            if(_body != null)
                _body.Dispose();
        }
        public void LoadContent(ContentManager Content)
        {
          
            Texture = Content.Load<Texture2D>(path);

            string tempPath = "PopImg";
            popTexture = Content.Load<Texture2D>(tempPath);

        }

        public void UnloadContent()
        {
            Content.Unload();
        }

        public void Initialize()
        {
           
        }

        public virtual Balloon Update(Unicorn unicorn)
        {
            float distance = Vector2.Distance(unicorn.GetCenterPos(), Position + new Vector2(width/2, height/2) );

            if (Math.Abs(distance) < 96)
            {
                isActive = false;
                Destroy();

                return this;
            }

            return null;

        }


        public virtual void Draw(SpriteBatch spriteBatch)
        {
         
            if (isActive)
            {
                if (path == "Balloon_yellow")
                {
                    CheckEdge();
                }

                spriteBatch.Draw(Texture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }

            if (!isActive)
            {
              
                   spriteBatch.Draw(popTexture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            }

           
        }

        private void CheckEdge()
        {


            if (startPosition.X + range < Position.X || Position.X< startPosition.X)
            {
            
                speed *= -1f;
            }

            Position.X += speed;
           

        }

        ~Balloon()
        {
            Destroy();
        }

     


    }
}
