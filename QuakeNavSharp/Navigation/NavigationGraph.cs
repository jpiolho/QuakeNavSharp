using QuakeNavSharp.Files;
using QuakeNavSharp.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace QuakeNavSharp.Navigation
{
    public class NavigationGraph : NavigationGraphBase
    {
        public class Edict
        {
            public Vector3 Mins { get; set; }
            public Vector3 Maxs { get; set; }
            public int EntityId { get; set; }
        }

        public class Link : IdentifiedComponentBase
        {
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
            Hazard = 1 << 5,
            CheckForFloor = 1 << 6,
            CheckForSolid = 1 << 7
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


        public override NavFileBase ToNavFileGeneric() => ToNavFile();
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
                        fileEdict.EntityId = edict.EntityId;

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

        public override NavJsonBase ToNavJsonGeneric() => ToNavJson();
        /// <summary>
        /// Converts this <see cref="NavigationGraphV14"/> to a <see cref="NavJsonV1"/> object.
        /// </summary>
        public NavJson ToNavJson()
        {
            var json = new NavJson();

            json.Nodes = new NavJson.Node[this.Nodes.Count];

            // Create nodes
            for (var nodeId = 0; nodeId < this.Nodes.Count; nodeId++)
            {
                var node = this.Nodes[nodeId];
                var jsonNode = new NavJson.Node();

                jsonNode.Flags = (int)node.Flags;
                jsonNode.Radius = node.Radius;
                jsonNode.Origin = node.Origin;

                // Create links
                jsonNode.Links = new NavJson.Link[node.Links.Count];
                for (var linkId = 0; linkId < node.Links.Count; linkId++)
                {
                    var link = node.Links[linkId];
                    var jsonLink = new NavJson.Link();

                    jsonLink.Target = link.Target.Origin;
                    jsonLink.Type = (int)link.Type;

                    // Create traversal
                    if (link.Traversal != null)
                    {
                        jsonLink.Traversal = new Vector3[] {
                            link.Traversal.Point1,
                            link.Traversal.Point2,
                            link.Traversal.Point3
                        };
                    }

                    // Create edict
                    if (link.Edict != null)
                    {
                        jsonLink.Edict = new NavJson.Edict()
                        {
                            Maxs = link.Edict.Maxs,
                            Mins = link.Edict.Mins,
                            EntityId = link.Edict.EntityId
                        };
                    }

                    jsonNode.Links[linkId] = jsonLink;
                }

                json.Nodes[nodeId] = jsonNode;
            }

            return json;
        }
    }
}
