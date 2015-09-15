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

using Game2;

namespace MonoGame_Test
{
    class Walls : IMyDestroyable
    {
        protected enum Orientation {HORIZONTAL, VERTICAL, EQUAL};

        public Body _body;            // body that is effected by physics
        public string Name;
        protected List<Body> _aura = new List<Body>();

        public Walls(string name, World world, float width, float height, float density, Vector2 pos,bool Static)
        {
            Name = name;
            _body = BodyFactory.CreateRectangle(world, width,height, density);
           
            _body.Position = pos;
            _body.IsStatic = Static;
            _body.Restitution = 0;
            _body.Friction = 0;
            _body.OnCollision += MyOnCollision;
            _body.BodyName = name;
            _body.UserData = ConvertUnits.ToDisplayUnits(new Vector2(width, height));

           /* _aura = BodyFactory.CreateCircle(world, 2.0f, 10.0f);
            _aura.Position = pos;
            _aura.BodyName = "aura";
            _aura.IsSensor = true;*/

            GameManager.getInstance().AddDestroyable(this);
        }

        public Walls(string magneticAttribute, World world, uint width, uint height, Point pos, bool Static)
        {
            float tileSize = GameManager.TILE_SIZE_CONV;
            float widthR = width*tileSize;
            float heightR = height*tileSize;
            Vector2 topLeft = new Vector2(pos.X*tileSize, pos.Y*tileSize);
            Vector2 mid = new Vector2(topLeft.X + widthR/2, topLeft.Y + heightR/2);
            InitializeBase(world, widthR, heightR, 10f, mid, Static);
            if(magneticAttribute != "")
            {
                Orientation facing;
                if(width > height)
                {
                    facing = Orientation.HORIZONTAL;
                }
                else if(width < height)
                {
                    facing = Orientation.VERTICAL;
                }
                else
                {
                    facing = Orientation.EQUAL;
                }

                float wallExtend = (GameManager.UnoToTiles * tileSize)/8;
                Action<string,float> createHorizontalSensor = delegate(string dir, float offsetY)
                {
                    CreateSensor(0,
                            magneticAttribute+"."+dir,
                            world,
                            widthR,
                            wallExtend,
                            new Vector2(mid.X, mid.Y+offsetY),
                            1.0f);
                };
                Action<string,float> createVerticalSensor = delegate(string dir, float offsetX)
                {
                    CreateSensor(0,
                            magneticAttribute+"."+dir,
                            world,
                            wallExtend,
                            heightR,
                            new Vector2(mid.X+offsetX, mid.Y),
                            1.0f);
                };

                switch(facing)
                {
                    case Orientation.HORIZONTAL:
                        {
                            float offset = (heightR/2 + wallExtend/2);
                            createHorizontalSensor("f", -offset);
                            createHorizontalSensor("c", +offset);
                        }
                        break;
                    case Orientation.VERTICAL:
                        {
                            float offset = (widthR/2 + wallExtend/2);
                            createVerticalSensor("r", -offset);
                            createVerticalSensor("l", +offset);
                        }
                        break;
                    case Orientation.EQUAL:
                    default:
                        {
                            float offsetH = (heightR/2 + wallExtend/2);
                            float offsetV = (widthR/2 + wallExtend/2);
                            createHorizontalSensor("f", -offsetH);
                            createHorizontalSensor("c", +offsetH);
                            createVerticalSensor("r", -offsetV);
                            createVerticalSensor("l", +offsetV);
                        }
                        break;
                }
            }
        }

        public Walls(World world, float width, float height, float density, Vector2 pos, bool Static)
        {
            InitializeBase(world, width, height, density, pos, Static);
        }

        private void InitializeBase(World world, float width, float height, float density, Vector2 pos, bool Static)
        {
            _body = BodyFactory.CreateRectangle(world, width, height, density);

            _body.Position = pos;
            _body.IsStatic = Static;
            _body.Restitution = 0;
            _body.Friction = 0;
            _body.OnCollision += MyOnCollision;
            _body.UserData = ConvertUnits.ToDisplayUnits(new Vector2(width, height));
        }

        public void CreateSensor(int numberofaura,string name,World world,float width,float height,Vector2 pos,float density)
        {
            Body body = BodyFactory.CreateRectangle(world, width, height, density);
            body.Position = pos;
            body.IsStatic = true;
            body.IsSensor = false;
            body.BodyName = name;
            body.UserData = ConvertUnits.ToDisplayUnits(new Vector2(width, height));
            _aura.Add(body);
            
        }

        public bool MyOnCollision(Fixture f1, Fixture f2, Contact contact)
        {
          
           


            return true;
        }

        public void Destroy()
        {
            _body.Dispose();
            foreach(var aura in _aura)
            {
                aura.Dispose();
            }
        }


    }
}
