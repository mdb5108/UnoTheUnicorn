using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGame_Test
{
    interface IActivator
    {
        void AddToActivatable(IActivateable a);
        void ActivateAll();
    }
}
