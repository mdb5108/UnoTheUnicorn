
using System.Collections.Generic;
using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.DebugView;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using pony;
using levels;
using Game2;

using xTile;
using xTile.Dimensions;
using xTile.Display;


namespace MonoGame_Test
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {

        public static int Height;
        public static int Width;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Unicorn Uno;
        float deltatime;
        DebugViewXNA debugview; // Debug view to see physics body
        World _world;           // World where all the physics work 
        Body _floor;            // body that is effected by physics
        Levels _levels;
        SpriteFont font;
        public string debugstring="Debuglog";
       
        
        // Initialize  balloons
        List<Balloon> balloons = new List<Balloon>();
        List<Vector2> balloonsPos = new List<Vector2> {new Vector2(100,500),new Vector2(200,420),new Vector2(300,540),new Vector2(400,500),new Vector2(500,520),new Vector2(600,510),new Vector2(700,470)};
        string[] colorType = { "Balloon_blue", "Balloon_green", "Balloon_orange", "Balloon_red", "Balloon_yellow" };
        List<string> colorPath;

        int amount;
        Texture2D backgroundTexture;

        private Map map;
        IDisplayDevice mapDisplayDevice;
        xTile.Dimensions.Rectangle viewport;

        //Add Songs
        Song song;
      
        public Game1()
        {
         //   Height = 1024;
          //  Width = 1536;

            Height = 700;
            Width = 900;
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = Height,
                PreferredBackBufferWidth = Width,
                IsFullScreen = true
            };

         
            Content.RootDirectory = "Content";

            // Initialize the balloon list
           amount = balloonsPos.Count;
           colorPath = new List<string>{colorType[0],colorType[1],colorType[2],colorType[3],colorType[4],colorType[2],colorType[0]};

            for (int i = 0; i < amount; i++)
            {
                Balloon balloon = new Balloon((Vector2)balloonsPos[i], Content, (string)colorPath[i]);
                balloons.Add(balloon);
            }

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Uno = new Unicorn(this, Content);
        
            // Create a world for physics to act
            _world = new World(new Vector2(0f, 9.8f));
           
            _levels = new Levels();


            // DebugView for Physics objects
            debugview = new DebugViewXNA(_world);
            base.Initialize();
            Components.Add(Uno);

            mapDisplayDevice = new XnaDisplayDevice(Content, graphics.GraphicsDevice);

            map.LoadTileSheets(mapDisplayDevice);

            viewport = new xTile.Dimensions.Rectangle(new Size(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here

            debugview.LoadContent(GraphicsDevice, Content);

            var stream = TitleContainer.OpenStream("Content\\Map1.tbin");
            map = xTile.Format.FormatManager.Instance.BinaryFormat.Load(stream);

            _levels.InitializeBoundaries(_world);
            _levels.Initialize(1, _world);
            Vector2 unopos = new Vector2(Width/2, 300);
            Uno.Initialize(Content.Load<Texture2D>("Uno"), unopos,_world);
            font = Content.Load<SpriteFont>("TestingFont");


            for (int i = 0; i < amount; i++)
            {
                Balloon b = (Balloon)balloons[i];
                b.LoadContent(Content);
            }
   

           
            backgroundTexture = Content.Load<Texture2D>("StoneDungeon_bg");

          //  backgroundMusic = Content.Load<Song>("test");
           // MediaPlayer.Play(backgroundMusic);
          //  this.song = Content.Load<Song>("test");
          //  MediaPlayer.Play(song);
            //  Uncomment the following line will also loop the song
            //  MediaPlayer.IsRepeating = true;
          //  MediaPlayer.MediaStateChanged += MediaPlayer_MediaStateChanged;
        }

       
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            //Uno.UnloadContent();
        }


        float t;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

             map.Update(gameTime.ElapsedGameTime.Milliseconds);

             deltatime = gameTime.ElapsedGameTime.Milliseconds;
             deltatime = deltatime / 1000;

             Uno.Update(gameTime,deltatime,_world);
       
            _world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, (1f / 30f)));


            for (int i = 0; i < balloons.Count; i++)
            {
                Balloon b = (Balloon)balloons[i];

                if (!b.isActive) {
                  
                    t +=  (float)deltatime;     // I'm working on it.
                    if (t >= .5f)
                    {
                        balloons.Remove(balloons[i]);
                        t = .0f;
                    }
                  
                }
            }


            // balloons pop check



            // balloons pop check & change color 

            for (int i = 0; i < balloons.Count; i++)
            {
                Balloon b = (Balloon)balloons[i];
                Balloon balloon = b.Update(Uno);

                if (balloon!=null)   // test color
                {
                    Uno.ChangeHair(balloon.path);
                   // Console.WriteLine(balloon.path);
                }

                balloon = null;
            }
            base.Update(gameTime);
        }

       
   
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Orange);
            spriteBatch.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend);

            map.Draw(mapDisplayDevice, viewport);

            var projection = Matrix.CreateOrthographicOffCenter(
                0f,
                ConvertUnits.ToSimUnits(graphics.GraphicsDevice.Viewport.Width),
                ConvertUnits.ToSimUnits(graphics.GraphicsDevice.Viewport.Height),
                0f,0f,1f);

            debugview.RenderDebugData(ref projection);

            Uno.Draw(spriteBatch);
          //  spriteBatch.DrawString(font, debugstring, new Vector2(Width/2, 20), Color.Tomato);

            spriteBatch.DrawString(font, Uno.contactbodyname, new Vector2(Width/2, 20), Color.Tomato);
           
            

            // test~
            for (int i = 0; i < balloons.Count; i++)
            {
               Balloon b = (Balloon)balloons[i];
               b.Draw(spriteBatch);
            }
 
            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
