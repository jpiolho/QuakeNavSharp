using QuakeNavSharp.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace QuakeNavSharp.Navigation
{
    public class NavigationGraph
    {
        public class Edict
        {
            public Vector3 Mins { get; set; }
            public Vector3 Maxs { get; set; }

            public int Targetname { get; set; }
            public int Classname { get; set; }
        }

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

        public enum LinkType : ushort
        {
            Walk = 0,
            LongJump = 1,
            Teleport = 2,
            WalkOffLedge = 3,
            Pusher = 4,
            BarrierJump = 5,
            Elevator = 6,
            Train = 7,
            ManualJump = 8,
            Unknown = 9
        }

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

        [Flags]
        public enum NodeFlags : ushort
        {
            None = 0,
            Teleporter = 1 << 0,
            Pusher = 1 << 1,
            ElevatorTop = 1 << 2,
            ElevatorBottom = 1 << 3,
            Underwater = 1 << 4,
            Hazard = 1 << 5
        }

        public class Traversal
        {
            public Vector3 Point1 { get; set; }
            public Vector3 Point2 { get; set; }
            public Vector3 Point3 { get; set; }
        }

        public IdentifiedComponentList<Node> Nodes { get; private set; }


        public NavigationGraph()
        {
            Nodes = new IdentifiedComponentList<Node>();
            Nodes.Removing += Nodes_Removing;
        }

        private void Nodes_Removing(object sender, int index, Node obj)
        {
            // Delete all links towards the node being removed
            foreach(var node in Nodes)
            {
                for (var i = node.Links.Count - 1; i >= 0;i--) {
                    if (node.Links[i].Target == obj)
                        node.Links.RemoveAt(i);
                }
            }
        }

        public Node NewNode()
        {
            var node = new Node(Nodes.Count, this);
            Nodes.Add(node);

            return node;
        }


        public void LoadFromFile(NavFile file)
        {
            Nodes.Clear();
            Nodes.Capacity = file.Nodes.Count;

            // Create nodes
            for (var i = 0; i < file.Nodes.Count; i++)
            {
                var fileNode = file.Nodes[i];
                var fileNodeOrigin = file.NodeOrigins[i];

                var node = this.NewNode();
                node.Flags = (NodeFlags)fileNode.Flags;
                node.Origin = fileNodeOrigin.Origin;
                node.Radius = fileNode.Radius;
            }

            // Make a dictionary of link id -> edict id
            var linkToEdict = new Dictionary<int, int>();
            for (var i = 0; i < file.Edicts.Count; i++)
            {
                var fileEdict = file.Edicts[i];
                linkToEdict[fileEdict.LinkId] = i;
            }

            // Create links
            for (var i = 0; i < file.Nodes.Count; i++)
            {
                var fileNode = file.Nodes[i];
                var node = Nodes[i];

                var linkStart = fileNode.ConnectionStartIndex;
                var linkEnd = linkStart + fileNode.ConnectionCount;
                for (var l = linkStart; l < linkEnd; l++)
                {
                    var fileLink = file.Links[l];
                    var link = node.NewLink();

                    link.Type = (LinkType)fileLink.Type;
                    link.Target = Nodes[fileLink.Destination];

                    // Add traversal if available
                    if (fileLink.TraversalIndex != 0xFFFF)
                    {
                        var fileTraversal = file.Traversals[fileLink.TraversalIndex];

                        link.Traversal = new Traversal()
                        {
                            Point1 = fileTraversal.Point1,
                            Point2 = fileTraversal.Point2,
                            Point3 = fileTraversal.Point3
                        };
                    }

                    // Add edict if available
                    if (linkToEdict.TryGetValue(l, out var edictId))
                    {
                        var fileEdict = file.Edicts[edictId];

                        link.Edict = new Edict()
                        {
                            Mins = fileEdict.Mins,
                            Maxs = fileEdict.Maxs,
                            Targetname = fileEdict.Targetname,
                            Classname = fileEdict.Classname
                        };
                    }

                }
            }
        }

        public static NavigationGraph FromNavFile(NavFile file)
        {
            var graph = new NavigationGraph();
            graph.LoadFromFile(file);
            return graph;
        }

        public NavFile ToNavFile()
        {
            var file = new NavFile();

            // Add nodes
            file.Nodes.Capacity = Nodes.Count;
            for (var i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                var fileNode = new NavFile.Node();

                fileNode.Flags = (ushort)node.Flags;
                fileNode.Radius = node.Radius;

                fileNode.ConnectionStartIndex = (ushort)file.Links.Count;
                fileNode.ConnectionCount = (ushort)node.Links.Count;

                // Add links
                for (var l = 0; l < node.Links.Count; l++)
                {
                    var link = node.Links[l];
                    var fileLink = new NavFile.Link();
                    var linkId = file.Links.Count;

                    fileLink.Type = (ushort)link.Type;
                    fileLink.Destination = (ushort)link.Target.Id;

                    // Add traversal if available
                    if (link.Traversal != null)
                    {
                        fileLink.TraversalIndex = (ushort)file.Traversals.Count;

                        var traversal = link.Traversal;
                        var fileTraversal = new NavFile.Traversal();

                        fileTraversal.Point1 = traversal.Point1;
                        fileTraversal.Point2 = traversal.Point2;
                        fileTraversal.Point3 = traversal.Point3;

                        file.Traversals.Add(fileTraversal);
                    }
                    else
                    {
                        // If none available, set 0xFFFF
                        fileLink.TraversalIndex = 0xFFFF;
                    }

                    // Add edict if available
                    if (link.Edict != null)
                    {
                        var edict = link.Edict;
                        var fileEdict = new NavFile.Edict();

                        fileEdict.LinkId = (ushort)linkId;
                        fileEdict.Mins = edict.Mins;
                        fileEdict.Maxs = edict.Maxs;
                        fileEdict.Classname = edict.Classname;
                        fileEdict.Targetname = edict.Targetname;

                        file.Edicts.Add(fileEdict);
                    }

                    file.Links.Add(fileLink);
                }


                file.NodeOrigins.Add(new NavFile.NodeOrigin()
                {
                    Origin = node.Origin
                });
                file.Nodes.Add(fileNode);
            }

            return file;
        }
    }
}
