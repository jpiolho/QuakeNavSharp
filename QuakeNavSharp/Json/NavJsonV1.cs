using QuakeNavSharp.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuakeNavSharp.Json
{
    public class NavJsonV1 : NavJsonBase
    {
        public class Edict
        {
            public Vector3 Mins { get; set; }
            public Vector3 Maxs { get; set; }
            public int Targetname { get; set; }
            public int Classname { get; set; }
        }

        public class Link
        {
            public Vector3 Target { get; set; }
            public int Type { get; set; }
            public Vector3[] Traversal { get; set; }
            public Edict Edict { get; set; }
        }

        public class MapInfo
        {
            public string Name { get; set; }
            public string Author { get; set; }
            public string Filename { get; set; }
            public string[] Urls { get; set; }
        }

        public class Node
        {
            public Vector3 Origin { get; set; }
            public int Flags { get; set; }
            public Link[] Links { get; set; }
            public int Radius { get; set; }
        }

        public int Version { get; private set; } = 1;
        public MapInfo Map { get; set; }
        public string[] Contributors { get; set; }
        public string Comments { get; set; }
        public Node[] Nodes { get; set; }

        /// <summary>
        /// Serializes the <see cref="NavJson"/> object to json.
        /// </summary>
        public override string ToJson()
        {
            return JsonSerializer.Serialize(this,_serializerOptions);
        }

        /// <summary>
        /// Deserializes a json string into a <see cref="NavJsonV1"/> object.
        /// </summary>
        public static NavJsonV1 FromJson(string json)
        {
            return JsonSerializer.Deserialize<NavJsonV1>(json, _serializerOptions);
        }


        public override NavigationGraphBase ToNavigationGraphGeneric() => ToNavigationGraph();

        /// <summary>
        /// Convert this <see cref="NavJson"/> to a <see cref="NavigationGraph"/>.
        /// </summary>
        public NavigationGraphV14 ToNavigationGraph()
        {
            var navigation = new NavigationGraphV14();


            var nodeOriginDictionary = new Dictionary<Vector3, NavigationGraphV14.Node>();

            // Add nodes
            foreach (var jsonNode in this.Nodes)
            {
                var node = navigation.NewNode();
                node.Flags = (NavigationGraphV14.NodeFlags)jsonNode.Flags;
                node.Radius = (ushort)jsonNode.Radius;
                node.Origin = jsonNode.Origin;

                nodeOriginDictionary[node.Origin] = node;
            }

            // Add links
            for (var i = 0; i < this.Nodes.Length; i++)
            {
                var node = navigation.Nodes[i];
                var jsonNode = this.Nodes[i];

                foreach (var jsonLink in jsonNode.Links)
                {
                    var link = node.NewLink();
                    link.Type = (NavigationGraphV14.LinkType)jsonLink.Type;
                    link.Target = nodeOriginDictionary[jsonLink.Target];

                    if (jsonLink.Traversal != null)
                    {
                        link.Traversal = new NavigationGraphV14.Traversal()
                        {
                            Point1 = jsonLink.Traversal[0],
                            Point2 = jsonLink.Traversal[1],
                            Point3 = jsonLink.Traversal[2]
                        };
                    }

                    if (jsonLink.Edict != null)
                    {
                        link.Edict = new NavigationGraphV14.Edict()
                        {
                            Mins = jsonLink.Edict.Mins,
                            Maxs = jsonLink.Edict.Maxs,
                            Targetname = jsonLink.Edict.Targetname,
                            Classname = jsonLink.Edict.Classname,
                        };
                    }
                }
            }

            return navigation;
        }
    }
}
