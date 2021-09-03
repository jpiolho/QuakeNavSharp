using System;
using System.Collections.Generic;
using System.Numerics;

namespace QuakeNavSharp.Navigation
{
    public sealed partial class NavigationGraph
    {
        public class Node : IdentifiedComponentBase
        {
            public const int MaximumLinks = 12;

            [Obsolete("Use Graph property instead")]
            public NavigationGraph Owner => Graph;

            /// <summary>
            /// Returns the <see cref="NavigationGraph" /> where this node belongs to.
            /// </summary>
            public NavigationGraph Graph { get; internal set; }

            public NodeFlags Flags { get; set; }
            public Vector3 Origin { get; set; }
            public ushort Radius { get; set; }
            public IdentifiedComponentList<Link> Links { get; private set; } = new IdentifiedComponentList<Link>(MaximumLinks);

            internal Node(int id, NavigationGraph graph)
            {
                this.Id = id;
                this.Graph = graph;
            }


            public Link NewLink()
            {
                if (Links.Count >= MaximumLinks)
                    throw new InvalidOperationException("Maximum number of links reached");

                var link = new Link(this);
                Links.Add(link);
                return link;
            }
        }
    }
}
