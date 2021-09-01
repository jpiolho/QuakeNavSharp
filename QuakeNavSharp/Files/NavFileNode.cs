namespace QuakeNavSharp.Files
{
    public class NavFileNode
    {
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
    }
}
