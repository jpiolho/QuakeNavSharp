using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuakeNavSharp.Files
{
    public class NavFile
    {
        private const int HEADER_SIZE = 20;


        public class Edict
        {
            internal const int HEADER_SIZE = 4;
            internal const int SIZE = 34;

            /// <summary>
            /// Link id to which this edict belongs to.
            /// </summary>
            public ushort LinkId { get; set; }

            /// <summary>
            /// Mins bounds of the edict.
            /// </summary>
            public Vector3 Mins { get; set; }

            /// <summary>
            /// Maxs bounds of the edict.
            /// </summary>
            public Vector3 Maxs { get; set; }

            /// <summary>
            /// Engine string id for the edict targetname.
            /// </summary>
            public int Targetname { get; set; }

            /// <summary>
            /// Engine string id for the edict classname.
            /// </summary>
            public int Classname { get; set; }

            public Edict Read(BinaryReader reader)
            {
                this.LinkId = reader.ReadUInt16();
                this.Mins = reader.ReadVector3();
                this.Maxs = reader.ReadVector3();
                this.Targetname = reader.ReadInt32();
                this.Classname = reader.ReadInt32();

                return this;
            }

            internal void Write(BinaryWriter writer)
            {
                writer.Write((ushort)this.LinkId);
                writer.Write((Vector3)this.Mins);
                writer.Write((Vector3)this.Maxs);
                writer.Write((int)this.Targetname);
                writer.Write((int)this.Classname);
            }

        }

        public class Link
        {
            internal const int SIZE = 6;

            /// <summary>
            /// Node id where this link is going to.
            /// </summary>
            public ushort Destination { get; set; }

            /// <summary>
            /// Link connection type.
            /// </summary>
            public ushort Type { get; set; }

            /// <summary>
            /// Index to which traversal should be used for this link. 0xFFFF if none is used.
            /// </summary>
            public ushort TraversalIndex { get; set; }

            internal void Write(BinaryWriter writer)
            {
                writer.Write((ushort)this.Destination);
                writer.Write((ushort)this.Type);
                writer.Write((ushort)this.TraversalIndex);
            }

            internal Link Read(BinaryReader reader)
            {
                this.Destination = reader.ReadUInt16();
                this.Type = reader.ReadUInt16();
                this.TraversalIndex = reader.ReadUInt16();

                return this;
            }
        }

        public class Node
        {
            internal const int SIZE = 8;

            /// <summary>
            /// Which flags apply to this node.
            /// </summary>
            public ushort Flags { get; set; }

            /// <summary>
            /// How many connections this node has.
            /// </summary>
            public ushort ConnectionCount { get; set; }

            /// <summary>
            /// Which connection index do the connections from this node begin.
            /// </summary>
            public ushort ConnectionStartIndex { get; set; }

            /// <summary>
            /// Node radius. Represented in the in-game editor via a green circle.
            /// </summary>
            public ushort Radius { get; set; }


            internal void Write(BinaryWriter writer)
            {
                writer.Write((ushort)this.Flags);
                writer.Write((ushort)this.ConnectionCount);
                writer.Write((ushort)this.ConnectionStartIndex);
                writer.Write((ushort)this.Radius);
            }

            internal Node Read(BinaryReader reader)
            {
                this.Flags = reader.ReadUInt16();
                this.ConnectionCount = reader.ReadUInt16();
                this.ConnectionStartIndex = reader.ReadUInt16();
                this.Radius = reader.ReadUInt16();

                return this;
            }
        }

        public class NodeOrigin
        {
            internal const int SIZE = 12;

            /// <summary>
            /// Position of the node.
            /// </summary>
            public Vector3 Origin { get; set; }

            internal NodeOrigin Read(BinaryReader reader)
            {
                this.Origin = reader.ReadVector3();
                return this;
            }

            internal void Write(BinaryWriter writer)
            {
                writer.Write((Vector3)this.Origin);
            }
        }

        public class Traversal
        {
            internal const int SIZE = 36;

            /// <summary>
            /// First point in the traversal.
            /// </summary>
            public Vector3 Point1 { get; set; }

            /// <summary>
            /// Second point in the traversal.
            /// </summary>
            public Vector3 Point2 { get; set; }

            /// <summary>
            /// Third point in the traversal.
            /// </summary>
            public Vector3 Point3 { get; set; }

            internal void Write(BinaryWriter writer)
            {
                writer.Write((Vector3)this.Point1);
                writer.Write((Vector3)this.Point2);
                writer.Write((Vector3)this.Point3);
            }

            internal Traversal Read(BinaryReader reader)
            {
                this.Point1 = reader.ReadVector3();
                this.Point2 = reader.ReadVector3();
                this.Point3 = reader.ReadVector3();

                return this;
            }
        }

        public List<Node> Nodes { get; private set; } = new List<Node>();
        public List<NodeOrigin> NodeOrigins { get; private set; } = new List<NodeOrigin>();
        public List<Link> Links { get; private set; } = new List<Link>();
        public List<Traversal> Traversals { get; private set; } = new List<Traversal>();
        public List<Edict> Edicts { get; private set; } = new List<Edict>();


        public void Save(Stream stream)
        {
            SaveAsync(stream).RunSynchronously();
        }

        public async Task SaveAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Write header
                writer.Write(new char[] { 'N', 'A', 'V', '2' });
                writer.Write((int)14); // Version
                writer.Write((int)Nodes.Count);
                writer.Write((int)Links.Count);
                writer.Write((int)Traversals.Count);

                // Write nodes
                foreach (var node in Nodes)
                    node.Write(writer);

                // Write node origins
                foreach (var nodeOrigin in NodeOrigins)
                    nodeOrigin.Write(writer);

                // Write links
                foreach (var link in Links)
                    link.Write(writer);

                // Write traversals
                foreach (var traversal in Traversals)
                    traversal.Write(writer);

                // Write edicts
                writer.Write((int)Edicts.Count);
                foreach (var edict in Edicts)
                    edict.Write(writer);

                // Finish up and write file to stream
                writer.Flush();
                ms.Position = 0;
                await ms.CopyToAsync(stream, cancellationToken);
            }
        }


        public static NavFile FromStream(Stream stream)
        {
            var task = FromStreamAsync(stream);
            task.RunSynchronously();
            return task.Result;
        }

        public static async Task<NavFile> FromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var file = new NavFile();

            using (var ms = new MemoryStream())
            using (var reader = new BinaryReader(ms))
            {
                // Read header
                await stream.ReadAsync(ms, HEADER_SIZE, true, cancellationToken);

                var magicNumber = reader.ReadChars(4);
                if (magicNumber[0] != 'N' || magicNumber[1] != 'A' || magicNumber[2] != 'V' || magicNumber[3] != '2')
                    throw new InvalidDataException("The provided stream is not in NAV2 format");

                var version = reader.ReadUInt32();
                if (version != 14)
                    throw new InvalidDataException($"Unsupported NAV2 version {version}");

                var nodeCount = reader.ReadUInt32();
                var linkCount = reader.ReadUInt32();
                var traversalCount = reader.ReadUInt32();


                // Read nodes
                await stream.ReadAsync(ms, Node.SIZE * (int)nodeCount, true, cancellationToken);

                file.Nodes.Capacity = (int)nodeCount;
                for (var i = 0; i < nodeCount; i++)
                    file.Nodes.Add(new Node().Read(reader));

                // Read node origins
                await stream.ReadAsync(ms, NodeOrigin.SIZE * (int)nodeCount, true, cancellationToken);

                file.NodeOrigins.Capacity = (int)nodeCount;
                for (var i = 0; i < nodeCount; i++)
                    file.NodeOrigins.Add(new NodeOrigin().Read(reader));

                // Read links
                await stream.ReadAsync(ms, Link.SIZE * (int)linkCount, true, cancellationToken);

                file.Links.Capacity = (int)linkCount;
                for (var i = 0; i < linkCount; i++)
                    file.Links.Add(new Link().Read(reader));

                // Read traversals
                await stream.ReadAsync(ms, Traversal.SIZE * (int)traversalCount, true, cancellationToken);

                file.Traversals.Capacity = (int)traversalCount;
                for (var i = 0; i < traversalCount; i++)
                    file.Traversals.Add(new Traversal().Read(reader));

                // Read edict header
                await stream.ReadAsync(ms, Edict.HEADER_SIZE, true, cancellationToken);
                var edictCount = reader.ReadUInt32();

                // Read edicts
                await stream.ReadAsync(ms, Edict.SIZE * (int)edictCount, true, cancellationToken);

                file.Edicts.Capacity = (int)edictCount;
                for (var i = 0; i < edictCount; i++)
                    file.Edicts.Add(new Edict().Read(reader));
            }


            return file;
        }
    }
}
