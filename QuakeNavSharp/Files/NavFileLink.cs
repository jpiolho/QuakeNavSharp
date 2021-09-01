namespace QuakeNavSharp.Files
{
    public class NavFileLink
    {
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
    }
}
