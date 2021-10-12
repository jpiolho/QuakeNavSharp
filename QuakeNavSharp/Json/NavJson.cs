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
    public class NavJson
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

        public int Version { get; set; }
        public MapInfo Map { get; set; }
        public string[] Contributors { get; set; }
        public string Comments { get; set; }
        public Node[] Nodes { get; set; }

        private static JsonSerializerOptions _serializerOptions;

        static NavJson()
        {
            _serializerOptions = new JsonSerializerOptions();
            _serializerOptions.Converters.Add(new Vector3JsonConverter());

            _serializerOptions.WriteIndented = true;
            _serializerOptions.IgnoreNullValues = true;
        }

        /// <summary>
        /// Serializes the <see cref="NavJson"/> object to json.
        /// </summary>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this,_serializerOptions);
        }

        /// <summary>
        /// Deserializes a json string into a <see cref="NavJson"/> object.
        /// </summary>
        public static NavJson FromJson(string json)
        {
            return JsonSerializer.Deserialize<NavJson>(json, _serializerOptions);
        }

        /// <summary>
        /// Convert this <see cref="NavJson"/> to a <see cref="NavigationGraph"/>.
        /// </summary>
        public NavigationGraph ToNavigationGraph()
        {
            return BuildGraphFromJson(this);
        }

        /// <summary>
        /// Converts a <see cref="NavigationGraph"/> to a <see cref="NavJson"/> object.
        /// </summary>
        public static NavJson FromNavigationGraph(NavigationGraph navigation)
        {
            return BuildJsonFromGraph(navigation);
        }

        private static NavigationGraph BuildGraphFromJson(NavJson json)
        {
            var navigation = new NavigationGraph();


            var nodeOriginDictionary = new Dictionary<Vector3, NavigationGraph.Node>();

            // Add nodes
            foreach (var jsonNode in json.Nodes)
            {
                var node = navigation.NewNode();
                node.Flags = (NavigationGraph.NodeFlags)jsonNode.Flags;
                node.Radius = (ushort)jsonNode.Radius;
                node.Origin = jsonNode.Origin;

                nodeOriginDictionary[node.Origin] = node;
            }

            // Add links
            for (var i = 0; i < json.Nodes.Length; i++)
            {
                var node = navigation.Nodes[i];
                var jsonNode = json.Nodes[i];

                foreach (var jsonLink in jsonNode.Links)
                {
                    var link = node.NewLink();
                    link.Type = (NavigationGraph.LinkType)jsonLink.Type;
                    link.Target = nodeOriginDictionary[jsonLink.Target];

                    if (jsonLink.Traversal != null)
                    {
                        link.Traversal = new NavigationGraph.Traversal()
                        {
                            Point1 = jsonLink.Traversal[0],
                            Point2 = jsonLink.Traversal[1],
                            Point3 = jsonLink.Traversal[2]
                        };
                    }

                    if (jsonLink.Edict != null)
                    {
                        link.Edict = new NavigationGraph.Edict()
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

        private static NavJson BuildJsonFromGraph(NavigationGraph navigation)
        {
            var json = new NavJson();

            json.Version = 1;
            json.Nodes = new Node[navigation.Nodes.Count];

            // Create nodes
            for (var nodeId = 0; nodeId < navigation.Nodes.Count; nodeId++)
            {
                var node = navigation.Nodes[nodeId];
                var jsonNode = new Node();

                jsonNode.Flags = (int)node.Flags;
                jsonNode.Radius = node.Radius;
                jsonNode.Origin = node.Origin;

                // Create links
                jsonNode.Links = new Link[node.Links.Count];
                for (var linkId = 0; linkId < node.Links.Count; linkId++)
                {
                    var link = node.Links[linkId];
                    var jsonLink = new Link();

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
                        jsonLink.Edict = new Edict()
                        {
                            Maxs = link.Edict.Maxs,
                            Mins = link.Edict.Mins,
                            Targetname = link.Edict.Targetname,
                            Classname = link.Edict.Classname
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
