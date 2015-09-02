using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using pony;
namespace MonoGame_Test
{
    public class Balloon
    {
        private Texture2D Texture;
        private string path;
        private Vector2 Position;
        public bool isActive;

        private Texture2D popTexture;
        private int popImageCount;
        private string color;

        public ContentManager Content;
        Body _body;
        public string name;
        public Balloon(Vector2 Position, ContentManager Content)
        {
            this.Position = Position;
            this.Content = Content;
            isActive = true;
            path = "blue_Balloon";

     
        }
        public Balloon(World world,Vector2 pos, ContentManager Content)
        {
            _body = BodyFactory.CreateCircle(world, 0.35f, 2.0f);
            _body.Position = ConvertUnits.ToSimUnits(pos.X+55,pos.Y+45);
            _body.BodyName = "baloon";
            
            this.Position = pos;
            this.Content = Content;
        }

        public void Destroy()
        {
            _body.Dispose();
        }
        public void LoadContent(ContentManager Content)
        {
            this.Content = Content;
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

        public void Update(Unicorn unicorn)
        {
            float distance = Vector2.Distance(unicorn.Position, Position );

            if (Math.Abs(distance) < 20)
                isActive = false;
                
                

        }

        public void Draw(SpriteBatch spriteBatch)
        {
         
            if (isActive)
            {
                spriteBatch.Draw(Texture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }

            if (!isActive)
            {
              
                   spriteBatch.Draw(popTexture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            }

           
        }

     


    }
}
