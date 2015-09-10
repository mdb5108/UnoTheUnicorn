
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MonoGame_Test
{
   

    class Pillar
    {
        private Vector2 Position;
        private ContentManager Content;
        private string path = "Pillar";
        private Texture2D PillarTexture;

        Pillar(Vector2 Position, ContentManager Content)
        {
            this.Position = Position;
            this.Content = Content;
        }


        public void LoadContent(ContentManager Content)
        {

            PillarTexture = Content.Load<Texture2D>(path);

        }

        public void Draw(SpriteBatch spriteBatch)
        {

                spriteBatch.Draw(PillarTexture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);  
        }
    }
}
