using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakeNavSharp.Navigation
{
    public sealed partial class NavigationGraph
    {
        public class Link : IdentifiedComponentBase
        {
            [Obsolete("Use Node property instead")]
            public Node Parent => Node;

            /// <summary>
            /// Returns the Node where this link belongs to.
            /// </summary>
            public Node Node { get; private set; }

            /// <summary>
            /// Returns the graph where this link belongs to.
            /// </summary>
            public NavigationGraph Graph => Node.Graph;


            public Node Target { get; set; }
            public LinkType Type { get; set; }
            public Traversal Traversal { get; set; }
            public Edict Edict { get; set; }

            internal Link(Node node)
            {
                this.Node = node;
            }
        }
    }
}
