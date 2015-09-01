
using System.Collections.Generic;
using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.DebugView;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using pony;
using levels;
using Game2;

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
        // Initialize a ballon
        List<Balloon> balloons = new List<Balloon>();
        List<Vector2> balloonsPos = new List<Vector2> {new Vector2(100,100),new Vector2(200,200),new Vector2(300,300)};
        int amount = 3;
       // Balloon b1; 
       

        public Game1()
        {
            Height = 800;
            Width = 1000;
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = Height,
                PreferredBackBufferWidth = Width,
                IsFullScreen = true
            };
            Content.RootDirectory = "Content";



            // Initialize the balloon list
            for (int i = 0; i < amount; i++)
            {
                Balloon balloon = new Balloon((Vector2)balloonsPos[i],Content);
                balloons.Add(balloon);
            }
        }

     
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Uno = new Unicorn(this);
        
            // Create a world for physics to act
            _world = new World(new Vector2(0f, 9.8f));
            //_world = new World(new Vector2(200, 0));
            _levels = new Levels();
            // DebugView for Physics objects
            debugview = new DebugViewXNA(_world);
            base.Initialize();
            Components.Add(Uno);
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here

            debugview.LoadContent(GraphicsDevice, Content);

            _levels.InitializeBoundaries(_world);
            Vector2 unopos = new Vector2(Width/2, 300);
            Uno.Initialize(Content.Load<Texture2D>("Uno"), unopos,_world);
            font = Content.Load<SpriteFont>("TestingFont");


            for (int i = 0; i < amount; i++)
            {
                Balloon b = (Balloon)balloons[i];
                b.LoadContent(Content);
            }
         //   b1 = new Balloon(new Vector2(500, 300),Content);
          //  b1.LoadContent(Content);
           
        }

       
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            //Uno.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

             deltatime = gameTime.ElapsedGameTime.Milliseconds;
             deltatime = deltatime / 1000;

             Uno.Update(gameTime,deltatime,_world);
       
            _world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, (1f / 30f)));
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

            debugview.RenderDebugData(ref projection);

            Uno.Draw(spriteBatch);
            spriteBatch.DrawString(font, Uno.contactbodyname, new Vector2(Width/2, 20), Color.Tomato);
            // test~
            for (int i = 0; i < amount; i++)
            {
                Balloon b = (Balloon)balloons[i];
                b.Draw(spriteBatch);

            }
         //   b1.Draw(spriteBatch);
            spriteBatch.End();
         

            base.Draw(gameTime);
        }
    }
}
