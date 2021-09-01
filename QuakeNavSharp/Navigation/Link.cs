using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakeNavSharp.Navigation
{
    public sealed partial class NavigationGraph
    {
        public class Link
        {
            public Node Parent { get; private set; }


            public Node Target { get; set; }
            public LinkType Type { get; set; }
            public Traversal Traversal { get; set; }
            public Edict Edict { get; set; }

            internal Link(Node parent)
            {
                this.Parent = parent;
            }
        }
    }
}
