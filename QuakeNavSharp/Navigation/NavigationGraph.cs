using QuakeNavSharp.Files;
using System.Collections.Generic;
using System.Linq;

namespace QuakeNavSharp.Navigation
{
    public sealed partial class NavigationGraph
    {
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
                var fileNode = new NavFileNode();

                fileNode.Flags = (ushort)node.Flags;
                fileNode.Radius = node.Radius;

                fileNode.ConnectionStartIndex = (ushort)file.Links.Count;
                fileNode.ConnectionCount = (ushort)node.Links.Count;

                // Add links
                for (var l = 0; l < node.Links.Count; l++)
                {
                    var link = node.Links[l];
                    var fileLink = new NavFileLink();
                    var linkId = file.Links.Count;

                    fileLink.Type = (ushort)link.Type;
                    fileLink.Destination = (ushort)link.Target.Id;

                    // Add traversal if available
                    if (link.Traversal != null)
                    {
                        fileLink.TraversalIndex = (ushort)file.Traversals.Count;

                        var traversal = link.Traversal;
                        var fileTraversal = new NavFileTraversal();

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
                        var fileEdict = new NavFileEdict();

                        fileEdict.LinkId = (ushort)linkId;
                        fileEdict.Mins = edict.Mins;
                        fileEdict.Maxs = edict.Maxs;
                        fileEdict.Classname = edict.Classname;
                        fileEdict.Targetname = edict.Targetname;

                        file.Edicts.Add(fileEdict);
                    }

                    file.Links.Add(fileLink);
                }


                file.NodeOrigins.Add(new NavFileNodeOrigin()
                {
                    Origin = node.Origin
                });
                file.Nodes.Add(fileNode);
            }

            return file;
        }
    }
}
