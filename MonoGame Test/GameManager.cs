using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics.Contacts;

using xTile;
using xTile.Dimensions;
using xTile.Display;


using MonoGame_Test;
using levels;
using pony;

namespace Game2
{
    class GameManager
    {

        // properties.
        public static GameManager gameManager;

      //  public int ScreenSizeX;
      //  public int ScreenSizeY;

        public ContentManager content;
        public World world;           // World where all the physics work

        public static readonly float TILE_SIZE = 32f;
        public static readonly float TILE_SIZE_CONV = ConvertUnits.ToSimUnits(TILE_SIZE);
        public static readonly uint UnoToTiles = 4;

        private List<Balloon> balloons = new List<Balloon>();

        private Texture2D unoTexture;

        public Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        private IDisplayDevice mapDisplayDevice;
        xTile.Dimensions.Rectangle viewport;

        private GraphicsDeviceManager graphics;

        private List<IMyUpdatable> updatable = new List<IMyUpdatable>();
        private List<IMyDrawable> drawable = new List<IMyDrawable>();

        public Unicorn uno
        {
            get;
            private set;
        }

        public uint level
        {
            get;
            private set;
        }

        private Map _map;
        public Map map
        {
            get {return _map;}
            private set {_map = value;}
        }

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

        public void AddUpdatable(IMyUpdatable u)
        {
            updatable.Add(u);
        }
        public void RemoveUpdatable(IMyUpdatable u)
        {
            updatable.Remove(u);
        }
        public void AddDrawable(IMyDrawable u)
        {
            drawable.Add(u);
        }
        public void RemoveDrawable(IMyDrawable d)
        {
            drawable.Remove(d);
        }

        public void AddBalloon(Balloon b)
        {
            balloons.Add(b);
        }

        public void RemoveBalloon(Balloon b)
        {
            balloons.Remove(b);
            if(balloons.Count == 0)
            {
                NextLevel();
            }
        }

        public ReadOnlyCollection<Balloon> GetBalloons()
        {
            return balloons.AsReadOnly();
        }

        public bool NextLevel()
        {
            balloons.Clear();
            updatable.Clear();
            drawable.Clear();

            level++;
            if(level < Levels.levelCount)
            {
                Vector2 unopos;
                Levels.getInstance().Initialize(level, world, content, out _map, out unopos);
                map.LoadTileSheets(mapDisplayDevice);
                foreach(Balloon b in GetBalloons())
                {
                    b.LoadContent(content);
                }

                uno.Initialize(unoTexture, unopos, world);
                return true;
            }
            else
            {
                level--;
            }
            return false;
        }

        public void Initialize(Game game, ContentManager content, ref GraphicsDeviceManager  graphics)
        {
            uno = new Unicorn(game, content);
            // Create a world for physics to act
            world = new World(new Vector2(0f, 9.8f));

            this.graphics = graphics;

            this.uno = uno;
            this.content = content;
            level = 0;

            mapDisplayDevice = new XnaDisplayDevice(content, graphics.GraphicsDevice);

            viewport = new xTile.Dimensions.Rectangle(new Size(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
        }

        public void LoadContent(ContentManager content)
        {
            Dictionary<string, string> wallTextures = new Dictionary<string, string>()
            {
                {"Wall", "PlatformTile_Stone"},
                {"Wall.b", "PlatformTile_Blue"},
                {"Wall.g", "PlatformTile_Green"},
                {"Wall.o", "PlatformTile_Orange"},
                {"Wall.y", "PlatformTile_Yellow"},
            };

            foreach(var namePath in wallTextures)
            {
                textures[namePath.Key] = content.Load<Texture2D>(namePath.Value);
            }

            Vector2 unopos;
            Levels.getInstance().Initialize(level, world, content, out _map, out unopos);
            map.LoadTileSheets(mapDisplayDevice);

            unoTexture = content.Load<Texture2D>("Uno");
            uno.Initialize(unoTexture, unopos, world);

            foreach(Balloon b in GetBalloons())
            {
                b.LoadContent(content);
            }
        }

        public void UnloadContent(ContentManager content)
        {
            content.Unload();
        }

        float t;
        public void Update(GameTime gameTime)
        {
             map.Update(gameTime.ElapsedGameTime.Milliseconds);

             float deltatime = gameTime.ElapsedGameTime.Milliseconds;
             deltatime = deltatime / 1000;

             uno.Update(gameTime, deltatime, world);

            world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, (1f / 30f)));

            foreach(var u in updatable)
            {
                u.Update(gameTime);
            }


            List<Balloon> removed = new List<Balloon>();
            foreach(Balloon b in GetBalloons())
            {
                if (!b.isActive) {

                    t +=  (float)deltatime;
                    if (t >= .5f)
                    {
                        removed.Add(b);
                        t = .0f;
                    }

                }
            }
            foreach(Balloon b in removed)
            {
                RemoveBalloon(b);
            }

            // balloons pop check & change color
            foreach(Balloon b in GetBalloons())
            {
                Balloon balloon = b.Update(uno);

                if (balloon!=null)
                {
                    uno.ChangeHair(balloon.path);
                }

                balloon = null;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            map.Draw(mapDisplayDevice, viewport);

            foreach(var d in drawable)
            {
                d.Draw(spriteBatch);
            }

            uno.Draw(spriteBatch);

            foreach(Balloon b in GetBalloons())
            {
               b.Draw(spriteBatch);
            }
        }
    }
}
