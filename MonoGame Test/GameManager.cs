using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics.Contacts;

namespace Game2
{
    class GameManager
    {

        // properties.
        private static GameManager gameManager;

      //  public int ScreenSizeX;
      //  public int ScreenSizeY;

        public float BackgroundMusicVol;
        public ContentManager content;

        public static readonly float TILE_SIZE = 32f;
        public static readonly float TILE_SIZE_CONV = ConvertUnits.ToSimUnits(TILE_SIZE);
        public static readonly uint UnoToTiles = 4;

        private GameManager()
        {
            // set the height & width of screen, change freely!!!
         //   ScreenSizeX = 800;
          //  ScreenSizeY = 560;

        }
        public static GameManager getInstance()
        {

            if (gameManager == null)
                gameManager = new GameManager();

            return gameManager;
        }



        public void LoadContent(ContentManager content)
        {
            this.content = content;
        }

        public void UnloadContent(ContentManager content)
        {
            content.Unload();
        }

        public void Update(GameTime gametime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
