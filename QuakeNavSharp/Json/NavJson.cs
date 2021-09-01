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
        public int Version { get; set; }
        public NavJsonMap Map { get; set; }
        public string[] Contributors { get; set; }
        public string Comments { get; set; }
        public NavJsonNode[] Nodes { get; set; }

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
            json.Nodes = new NavJsonNode[navigation.Nodes.Count];

            // Create nodes
            for (var nodeId = 0; nodeId < navigation.Nodes.Count; nodeId++)
            {
                var node = navigation.Nodes[nodeId];
                var jsonNode = new NavJsonNode();

                jsonNode.Flags = (int)node.Flags;
                jsonNode.Radius = node.Radius;
                jsonNode.Origin = node.Origin;

                // Create links
                jsonNode.Links = new NavJsonLink[node.Links.Count];
                for (var linkId = 0; linkId < node.Links.Count; linkId++)
                {
                    var link = node.Links[linkId];
                    var jsonLink = new NavJsonLink();

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
                        jsonLink.Edict = new NavJsonEdict()
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
