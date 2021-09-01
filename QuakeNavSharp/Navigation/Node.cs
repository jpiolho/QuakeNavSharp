using System;
using System.Collections.Generic;
using System.Numerics;

namespace QuakeNavSharp.Navigation
{
    public sealed partial class NavigationGraph
    {
        public class Node
        {
            public const int MaximumLinks = 12;

            public NavigationGraph Owner { get; internal set; }
            public int Id { get; internal set; }

            public NodeFlags Flags { get; set; }
            public Vector3 Origin { get; set; }
            public ushort Radius { get; set; }
            public List<Link> Links { get; private set; } = new List<Link>(MaximumLinks);

            internal Node(int id, NavigationGraph owner)
            {
                this.Id = id;
                this.Owner = owner;
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
