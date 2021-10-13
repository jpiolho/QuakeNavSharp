using System.IO;

namespace QuakeNavSharp.Files
{
    public abstract class NavFileComponent<T>
    {
        internal abstract T Read(BinaryReader reader);
        internal abstract void Write(BinaryWriter writer);
    }
}
