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
        private Vector2 Position;
        private bool isActive;

        private string path;
        private string color;

        public ContentManager Content;
                
        public Balloon(Vector2 Position, ContentManager Content)
        {
            this.Position = Position;
            this.Content = Content;
            isActive = true;
        }

        public void LoadContent(ContentManager Content)
        {
            this.Content = Content; 
             Texture = Content.Load<Texture2D>("blue_Balloon");
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
           
            if (Math.Abs(distance) < 1)
                isActive = false;

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
           
        }

    }
}
