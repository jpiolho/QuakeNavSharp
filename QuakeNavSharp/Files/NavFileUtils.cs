using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuakeNavSharp.Files
{
    public static class NavFileUtils
    {
        private const int STUB_SIZE = 4 + 4;

        private static Dictionary<uint, Type> _versionToTypeDictionary = new Dictionary<uint, Type>()
        {
            { 17, typeof(NavFile) },
            { 15, typeof(NavFileV15) },
            { 14, typeof(NavFileV14) }
        };


        public static async Task<NavFileBase> LoadAnyVersionAsync(Stream stream,CancellationToken cancellationToken = default)
        {
            using (var ms = new MemoryStream())
            using (var reader = new BinaryReader(ms))
            {
                // Read a stub header
                await stream.ReadAsync(ms, STUB_SIZE , true, cancellationToken);

                var magicNumber = reader.ReadChars(4);
                if (magicNumber[0] != 'N' || magicNumber[1] != 'A' || magicNumber[2] != 'V' || magicNumber[3] != '2')
                    throw new InvalidDataException("The provided stream is not in NAV2 format");

                // Figure out the specific version
                var version = reader.ReadUInt32();
                if(!_versionToTypeDictionary.TryGetValue(version,out var type))
                    throw new InvalidDataException($"Unsupported NAV2 version {version}");

                // Create the right NavFile instance
                var navFile = (NavFileBase)Activator.CreateInstance(type);

                // Read the file
                stream.Position = 0;
                await navFile.LoadAsync(stream, cancellationToken);

                return navFile;
            }
        }
    }
}
