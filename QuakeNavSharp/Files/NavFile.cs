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
        private const int NODE_SIZE = 8;
        private const int NODEORIGIN_SIZE = 12;
        private const int LINK_SIZE = 6;
        private const int TRAVERSAL_SIZE = 36;
        private const int EDICT_HEADER_SIZE = 4;
        private const int EDICT_SIZE = 34;

        public List<NavFileNode> Nodes { get; private set; } = new List<NavFileNode>();
        public List<NavFileNodeOrigin> NodeOrigins { get; private set; } = new List<NavFileNodeOrigin>();
        public List<NavFileLink> Links { get; private set; } = new List<NavFileLink>();
        public List<NavFileTraversal> Traversals { get; private set; } = new List<NavFileTraversal>();
        public List<NavFileEdict> Edicts { get; private set; } = new List<NavFileEdict>();


        public void Save(Stream stream)
        {
            SaveAsync(stream).RunSynchronously();
        }

        public async Task SaveAsync(Stream stream,CancellationToken cancellationToken = default)
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
                foreach(var node in Nodes)
                {
                    writer.Write((ushort)node.Flags);
                    writer.Write((ushort)node.ConnectionCount);
                    writer.Write((ushort)node.ConnectionStartIndex);
                    writer.Write((ushort)node.Radius);
                }

                // Write node origins
                foreach(var nodeOrigin in NodeOrigins)
                {
                    writer.Write((Vector3)nodeOrigin.Origin);
                }

                // Write links
                foreach(var link in Links)
                {
                    writer.Write((ushort)link.Destination);
                    writer.Write((ushort)link.Type);
                    writer.Write((ushort)link.TraversalIndex);
                }

                // Write traversals
                foreach(var traversal in Traversals)
                {
                    writer.Write((Vector3)traversal.Point1);
                    writer.Write((Vector3)traversal.Point2);
                    writer.Write((Vector3)traversal.Point3);
                }

                // Write edicts
                writer.Write((int)Edicts.Count);
                foreach(var edict in Edicts)
                {
                    writer.Write((ushort)edict.LinkId);
                    writer.Write((Vector3)edict.Mins);
                    writer.Write((Vector3)edict.Maxs);
                    writer.Write((int)edict.Targetname);
                    writer.Write((int)edict.Classname);
                }

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
                ms.Position = 0;
                await stream.ReadAsync(ms, HEADER_SIZE, cancellationToken);
                ms.Position = 0;
                
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
                ms.Position = 0;
                await stream.ReadAsync(ms, NODE_SIZE * (int)nodeCount, cancellationToken);
                ms.Position = 0;

                file.Nodes.Capacity = (int)nodeCount;
                for(var i=0;i<nodeCount;i++)
                {
                    var node = new NavFileNode();

                    node.Flags = reader.ReadUInt16();
                    node.ConnectionCount = reader.ReadUInt16();
                    node.ConnectionStartIndex = reader.ReadUInt16();
                    node.Radius = reader.ReadUInt16();

                    file.Nodes.Add(node);
                }

                // Read node origins
                ms.Position = 0;
                await stream.ReadAsync(ms, NODEORIGIN_SIZE * (int)nodeCount, cancellationToken);
                ms.Position = 0;

                file.NodeOrigins.Capacity = (int)nodeCount;
                for(var i=0;i<nodeCount;i++)
                {
                    var origin = new NavFileNodeOrigin();

                    origin.Origin = reader.ReadVector3();

                    file.NodeOrigins.Add(origin);
                }

                // Read links
                ms.Position = 0;
                await stream.ReadAsync(ms, LINK_SIZE * (int)linkCount, cancellationToken);
                ms.Position = 0;

                file.Links.Capacity = (int)linkCount;
                for(var i = 0; i < linkCount; i++)
                {
                    var link = new NavFileLink();

                    link.Destination = reader.ReadUInt16();
                    link.Type = reader.ReadUInt16();
                    link.TraversalIndex = reader.ReadUInt16();

                    file.Links.Add(link);
                }

                // Read traversals
                ms.Position = 0;
                await stream.ReadAsync(ms, TRAVERSAL_SIZE * (int)traversalCount, cancellationToken);
                ms.Position = 0;

                file.Traversals.Capacity = (int)traversalCount;
                for(var i = 0; i < traversalCount; i++)
                {
                    var traversal = new NavFileTraversal();

                    traversal.Point1 = reader.ReadVector3();
                    traversal.Point2 = reader.ReadVector3();
                    traversal.Point3 = reader.ReadVector3();

                    file.Traversals.Add(traversal);
                }

                // Read edict header
                ms.Position = 0;
                await stream.ReadAsync(ms, EDICT_HEADER_SIZE, cancellationToken);
                ms.Position = 0;
                var edictCount = reader.ReadUInt32();

                // Read edicts
                ms.Position = 0;
                await stream.ReadAsync(ms, EDICT_SIZE * (int)edictCount, cancellationToken);
                ms.Position = 0;

                file.Edicts.Capacity = (int)edictCount;
                for(var i=0;i<edictCount;i++)
                {
                    var edict = new NavFileEdict();

                    edict.LinkId = reader.ReadUInt16();
                    edict.Mins = reader.ReadVector3();
                    edict.Maxs = reader.ReadVector3();
                    edict.Targetname = reader.ReadInt32();
                    edict.Classname = reader.ReadInt32();

                    file.Edicts.Add(edict);
                }
            }


            return file;
        }
    }
}
