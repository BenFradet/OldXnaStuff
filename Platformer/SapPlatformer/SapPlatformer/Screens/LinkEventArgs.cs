using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platformer.Screens
{
    class LinkEventArgs : EventArgs
    {
        string link;
        public string Link
        {
            get { return link; }
        }

        public LinkEventArgs(string link)
        {
            this.link = link;
        }
        public LinkEventArgs() { }
    }
}
