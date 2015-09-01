using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using pony;
namespace Game2
{
    class Balloon
    {
        private Texture2D Texture;
        private string path;
        private Vector2 Position;
        public bool isActive;

        private Texture2D popTexture;
        private int popImageCount;
        private string color;

        public ContentManager Content;
                
        public Balloon(Vector2 Position, ContentManager Content)
        {
            this.Position = Position;
            this.Content = Content;
            isActive = true;
            path = "blue_Balloon";

     
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
