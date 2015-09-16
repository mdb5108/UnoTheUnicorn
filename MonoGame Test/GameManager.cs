using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

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

        public enum ManagerState{
            UNSPECIFIED,
            UPDATING,
            DRAWING,
        };
        public ManagerState State
        {
            get;
            private set;
        }
        private bool nextLevelCallDelay;

        private List<IMyDestroyable> destroyable = new List<IMyDestroyable>();

        private List<IMyUpdatable> updatable = new List<IMyUpdatable>();
        private List<IMyUpdatable> bufferUpdatable = new List<IMyUpdatable>();
        private List<IMyUpdatable> bufferUpdatableRemove = new List<IMyUpdatable>();
        private List<IMyDrawable> drawable = new List<IMyDrawable>();
        private List<IMyDrawable> bufferDrawable = new List<IMyDrawable>();
        private List<IMyDrawable> bufferDrawableRemove = new List<IMyDrawable>();

        public ExitPortal exit
        {
            get;
            private set;
        }

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

        public void AddDestroyable(IMyDestroyable d)
        {
            destroyable.Add(d);
        }
        public void RemoveDestroyable(IMyDestroyable d)
        {
            destroyable.Remove(d);
        }
        public void AddUpdatable(IMyUpdatable u)
        {
            if(State != ManagerState.UPDATING)
                updatable.Add(u);
            else
                bufferUpdatable.Add(u);
        }
        public void RemoveUpdatable(IMyUpdatable u)
        {
            if(State != ManagerState.UPDATING)
                updatable.Remove(u);
            else
                bufferUpdatableRemove.Add(u);
        }
        public void AddDrawable(IMyDrawable d)
        {
            if(State != ManagerState.DRAWING)
                drawable.Add(d);
            else
                bufferDrawable.Add(d);
        }
        public void RemoveDrawable(IMyDrawable d)
        {
            if(State != ManagerState.DRAWING)
                drawable.Remove(d);
            else
                bufferDrawableRemove.Add(d);
        }

        public void AddBalloon(Balloon b)
        {
            balloons.Add(b);
        }

        public void RemoveBalloon(Balloon b)
        {
            balloons.Remove(b);
            if(balloons.Count == 0 && exit == null)
            {
                NextLevel();
            }
        }

        public ReadOnlyCollection<Balloon> GetBalloons()
        {
            return balloons.AsReadOnly();
        }

        private void ClearLevel()
        {
            foreach(var b in balloons)
            {
                b.Destroy();
            }
            foreach(var d in destroyable)
            {
                d.Destroy();
            }
            balloons.Clear();
            updatable.Clear();
            drawable.Clear();
        }

        private void LoadLevel(uint level)
        {
            ClearLevel();
            Vector2 unopos;
            ExitPortal tmpExitPortal;
            Levels.getInstance().Initialize(level, world, content, out _map, out unopos, out tmpExitPortal);
            exit = tmpExitPortal;
            map.LoadTileSheets(mapDisplayDevice);
            foreach(Balloon b in GetBalloons())
            {
                b.LoadContent(content);
            }

            uno.Initialize(unoTexture, unopos, world);
        }

        public bool NextLevel()
        {
            bool ret = false;
            level++;
            if(level < Levels.levelCount)
            {
                ret = true;
            }
            else
            {
                level--;
                ret = false;
            }

            if(State == ManagerState.UNSPECIFIED)
            {
                LoadLevel(level);
            }
            else
            {
                nextLevelCallDelay = true;
            }

            return ret;
        }
    
        public bool BackLevel()
        {
            bool ret = false;
            if (level >= 1) level--;

            if(State == ManagerState.UNSPECIFIED)
            {
                LoadLevel(level);
            }else
            {
                nextLevelCallDelay = true;
            }
            return ret;
        }

        public void ResetLevel()
        {
            LoadLevel(level);
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

            nextLevelCallDelay = false;
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

            textures["ExitPortal.0"] = content.Load<Texture2D>("Exit_Portal_01");
            textures["ExitPortal.1"] = content.Load<Texture2D>("Exit_Portal_02");
            textures["ExitPortal.2"] = content.Load<Texture2D>("Exit_Portal_03");
            textures["ExitPortal.3"] = content.Load<Texture2D>("Exit_Portal_04");
            textures["ExitPortal.4"] = content.Load<Texture2D>("Exit_Portal_05");
            unoTexture = content.Load<Texture2D>("Uno_base");

            LoadLevel(level);
        }

        public void UnloadContent(ContentManager content)
        {
            content.Unload();
        }

        float t;
        bool pressedN = false;
        bool pressedB = false;
        public void Update(GameTime gameTime)
        {
             map.Update(gameTime.ElapsedGameTime.Milliseconds);

             float deltatime = gameTime.ElapsedGameTime.Milliseconds;
             deltatime = deltatime / 1000;

             uno.Update(gameTime, deltatime, world);

            world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, (1f / 30f)));

            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                ResetLevel();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.N))
            {
              if(!pressedN) NextLevel();
                pressedN = true;
            }else if(Keyboard.GetState().IsKeyUp(Keys.N))
            {
                pressedN = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.B))
            {
                if (!pressedB) BackLevel();
                pressedB = true;
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.B))
            {
                pressedB = false;
            }


            State = ManagerState.UPDATING;
            foreach(var u in updatable)
            {
                u.Update(gameTime);

            }
            foreach(var u in bufferUpdatable)
            {
                updatable.Add(u);
            }
            bufferUpdatable.Clear();
            foreach(var u in bufferUpdatableRemove)
            {
                updatable.Remove(u);
            }
            bufferUpdatableRemove.Clear();
            State = ManagerState.UNSPECIFIED;


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
                    uno.EyePop();
                }

                balloon = null;
            }

            //load level after the updating so collections are not modified while
            //updating
            if(nextLevelCallDelay)
            {
                nextLevelCallDelay = false;
                LoadLevel(level);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            map.Draw(mapDisplayDevice, viewport);

            State = ManagerState.DRAWING;
            foreach(var d in drawable)
            {
                d.Draw(spriteBatch);
            }
            foreach(var d in bufferDrawable)
            {
                drawable.Add(d);
            }
            bufferDrawable.Clear();
            foreach(var d in bufferDrawableRemove)
            {
                drawable.Remove(d);
            }
            bufferDrawableRemove.Clear();
            State = ManagerState.UNSPECIFIED;

            uno.Draw(spriteBatch);

            foreach(Balloon b in GetBalloons())
            {
               b.Draw(spriteBatch);
            }
        }
    }
}
