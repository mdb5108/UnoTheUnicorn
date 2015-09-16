using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.DebugView;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;


using Game2;

namespace MonoGame_Test
{
    class MovableWall : Walls, IActivateable, IMyUpdatable, IMyDrawable
    {
        public Vector2 inactivePosition;
        public Vector2 activatedPosition;
        public float activationTime;

        private float curActivatedTime;

        private float tileWidth;
        private float tileHeight;

        private float width;
        private float height;

        private string color;

        private bool activated;

        private float speed;
        private Vector2 direction;

        private Vector2 bodyOffset;
        private Vector2[] auraOffset;

        private Vector2 _Position;

        private ContentManager Content;

        private SoundEffect wallMovingAudio;
        private SoundEffectInstance wallMovingCopy;

        public Vector2 Position
        {
            get
            {
                return _Position;
            }
            set
            {
                _Position = value;
                _body.SetTransform(ConvertUnits.ToSimUnits(value + bodyOffset), _body.Rotation);
                int i = 0;
                foreach(Body aura in _aura)
                {
                    aura.SetTransform(ConvertUnits.ToSimUnits(value + auraOffset[i]), aura.Rotation);
                    i++;
                }
            }
        }

        public void SetLinearVelocity(Vector2 velocity)
        {
            _body.LinearVelocity = velocity;
            foreach(var aura in _aura)
            {
                aura.LinearVelocity = velocity;
            }
        }

        public Vector2 TopLeftCorner
        {
            get
            {
                return new Vector2(_Position.X - width/2, _Position.Y - height/2);
            }
        }

        public MovableWall(string magneticAttribute, World world, uint width, uint height, Point pos, float activationTime, Point activatedPosition, float speed, ContentManager Content)
        : base(magneticAttribute, world, width, height, pos, false)
        {
            float tileSize = GameManager.TILE_SIZE;
            this.activationTime = activationTime;
            this.curActivatedTime = 0;
            this.activated = false;

            this.speed = speed;

            this.Content = Content;

            wallMovingAudio = Content.Load<SoundEffect>("sfx/blocks_moving");
            wallMovingCopy = wallMovingAudio.CreateInstance();

            _body.BodyType = BodyType.Kinematic;
            foreach(var aura in _aura)
            {
                aura.BodyType = BodyType.Kinematic;
            }

            string[] words = magneticAttribute.Split('.');
            if(words.Length <= 1)
                color = words[0];

            this.tileWidth = width;
            this.tileHeight = height;

            this.width = width*tileSize;
            this.height = height*tileSize;

            this.activatedPosition = new Vector2(activatedPosition.X*tileSize + this.width/2, activatedPosition.Y*tileSize + this.height/2);
            this.inactivePosition = new Vector2(pos.X*tileSize + this.width/2, pos.Y*tileSize + this.height/2);
            this._Position = this.inactivePosition;
            bodyOffset = ConvertUnits.ToDisplayUnits(_body.Position) - this._Position;
            auraOffset = (from aura in _aura select (ConvertUnits.ToDisplayUnits(aura.Position) - this._Position)).ToArray();

            this.direction = Vector2.Normalize(this.activatedPosition - this._Position);

            GameManager.getInstance().AddUpdatable(this);
            GameManager.getInstance().AddDrawable(this);
        }

        public void Draw(SpriteBatch spritebatch)
        {
            Texture2D texture;
            if(color == "")
                texture = GameManager.getInstance().textures["Wall"];
            else
                texture = GameManager.getInstance().textures["Wall."+color];
            for(int x = 0; x < tileWidth; x++)
            {
                for(int y = 0; y < tileHeight; y++)
                {
                    spritebatch.Draw(texture, TopLeftCorner + new Vector2(x*GameManager.TILE_SIZE, y*GameManager.TILE_SIZE), null, Color.White);
                }
            }
        }

        private bool MoveTowards(GameTime gameTime, Vector2 targetPosition)
        {
            if(Position == targetPosition)
                return true;

            Vector2 difference = ConvertUnits.ToSimUnits(targetPosition - Position);
            Vector2 velocity = speed*Vector2.Normalize(difference);
            if(difference.Length() >= (((float)gameTime.ElapsedGameTime.TotalSeconds)*velocity).Length())
            {
                SetLinearVelocity(velocity);
                return false;
            }
            else
            {
                Position = targetPosition;
                SetLinearVelocity(Vector2.Zero);
                return true;
            }
        }

        public void Update(GameTime gameTime)
        {
            if(activated)
            {
                curActivatedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if(curActivatedTime > activationTime)
                {
                    activated = false;
                }
                else
                {
                    MoveTowards(gameTime, activatedPosition);
                }
            }
            else
            {
                MoveTowards(gameTime, inactivePosition);
            }
            _Position = ConvertUnits.ToDisplayUnits(_body.Position) - bodyOffset;
        }

        public void Activate()
        {
            if(!activated)
            {
                activated = true;
                curActivatedTime = 0;
            }


            if (activated && wallMovingCopy.State != SoundState.Playing)
            {
                wallMovingCopy.Play();
               
            }

        }

        ~MovableWall()
        {
            GameManager.getInstance().RemoveUpdatable(this);
            GameManager.getInstance().RemoveDrawable(this);
        }
    }
}
