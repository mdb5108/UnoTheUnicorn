
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
using Microsoft.Xna.Framework.Audio;

using pony;
using levels;
using Game2;

using xTile;
using xTile.Dimensions;
using xTile.Display;


namespace MonoGame_Test
{

    public class Game1 : Game
    {

        public static int Height;
        public static int Width;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        float deltatime;
        DebugViewXNA debugview; // Debug view to see physics body
        SpriteFont font;
        public string debugstring="Debuglog";


        SoundEffect bgs;
     
        public Game1()
        {
            Height = 1024;
            Width = 1536;

            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = Height,
                PreferredBackBufferWidth = Width,
                IsFullScreen = true
            };

         
            Content.RootDirectory = "Content";

            this.Window.Position = new Point(0, 0);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            GameManager.getInstance().Initialize(this, Content, ref graphics);
           
            // DebugView for Physics objects
            debugview = new DebugViewXNA(GameManager.getInstance().world);
            base.Initialize();
            Components.Add(GameManager.getInstance().uno);
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here

            GameManager.getInstance().LoadContent(Content);

            debugview.LoadContent(GraphicsDevice, Content);

            font = Content.Load<SpriteFont>("TestingFont");

          //  bgs = Content.Load<SoundEffect>("1");

        }

       
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            //Uno.UnloadContent();
            GameManager.getInstance().UnloadContent(Content);
            
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            GameManager.getInstance().Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Orange);
            spriteBatch.Begin(SpriteSortMode.Deferred,BlendState.AlphaBlend);

            var projection = Matrix.CreateOrthographicOffCenter(
                0f,
                ConvertUnits.ToSimUnits(graphics.GraphicsDevice.Viewport.Width),
                ConvertUnits.ToSimUnits(graphics.GraphicsDevice.Viewport.Height),
                0f,0f,1f);

            GameManager.getInstance().Draw(spriteBatch);
            //debugview.RenderDebugData(ref projection);

          //  spriteBatch.DrawString(font, debugstring, new Vector2(Width/2, 20), Color.Tomato);

            spriteBatch.DrawString(font, GameManager.getInstance().uno.debugString, new Vector2(Width/2, 20), Color.Tomato);
 
            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
