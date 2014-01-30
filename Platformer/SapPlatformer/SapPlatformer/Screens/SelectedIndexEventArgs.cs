using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platformer.Screens
{
    class SelectedIndexEventArgs : EventArgs
    {
        int index;
        public int Index
        {
            get { return index; }
        }

        public SelectedIndexEventArgs(int index)
        {
            this.index = index;
        }
    }
}
