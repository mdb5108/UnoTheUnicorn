using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics.Contacts;

using Game2;

namespace MonoGame_Test
{
    class ExitPortal :  IMyUpdatable, IMyDrawable
    {
        private static readonly int PHASE_COUNT = 5;
        private int phase;

        private Vector2 position;
        private static float Width = 256;
        private static float Height = 256;
        private static readonly Vector2 CENTER_OFFSET = new Vector2(Width/2, Height/2);
        private static readonly float ENTER_RADIUS = 128;

        public Vector2 CenterPosition
        {
            get
            {
                return position + CENTER_OFFSET;
            }
        }

        public ExitPortal(Point pos)
        {
            GameManager gm = GameManager.getInstance();

            position = new Vector2(GameManager.TILE_SIZE*pos.X, GameManager.TILE_SIZE*pos.Y);

            gm.AddDrawable(this);
            gm.AddUpdatable(this);
            phase = 0;
        }

        ~ExitPortal()
        {
            GameManager.getInstance().RemoveDrawable(this);
            GameManager.getInstance().RemoveUpdatable(this);
        }

        public void SetPhase(int phase)
        {
            this.phase = phase;
        }

        public void IncreasePhase()
        {
            if(phase < PHASE_COUNT-1)
                phase++;
        }

        public void Update(GameTime gameTime)
        {
            if(phase == PHASE_COUNT -1)
            {
                if(Vector2.Distance(GameManager.getInstance().uno.CenterPosition, CenterPosition) < ENTER_RADIUS)
                {
                    GameManager.getInstance().NextLevel();
                }
            }
        }

        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Draw(GameManager.getInstance().textures["ExitPortal."+phase], position, null, Color.White);
        }
    }
}
