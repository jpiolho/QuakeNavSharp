using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace QuakeNavSharp.Files
{
    public abstract class NavFileBase
    {
        /// <summary>
        /// Returns the version of this file.
        /// </summary>
        public abstract int Version { get; }

        public abstract void Load(Stream stream);
        public abstract Task LoadAsync(Stream stream,CancellationToken cancellationToken = default);

        public abstract void Save(Stream stream);
        public abstract Task SaveAsync(Stream stream, CancellationToken cancellationToken = default);
    }
}
