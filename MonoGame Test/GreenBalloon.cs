using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using pony;

namespace MonoGame_Test
{
    class GreenBalloon : Balloon, IActivator
    {
        List<IActivateable> activatable;
        bool activatedOnce;

        public GreenBalloon(Point position, ContentManager content)
            : base(position, content, "g", 0, 0)
        {
            activatable = new List<IActivateable>();
            activatedOnce = false;
        }

        public override Balloon Update(Unicorn unicorn)
        {
            if(!activatedOnce && !isActive)
            {
                activatedOnce = true;
                ActivateAll();
            }

            return base.Update(unicorn);
        }

        public void ActivateAll()
        {
            foreach(var a in activatable)
            {
                a.Activate();
            }
        }

        public void AddToActivatable(IActivateable a)
        {
            activatable.Add(a);
        }
    }
}
