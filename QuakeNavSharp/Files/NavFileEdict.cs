using System.Numerics;

namespace QuakeNavSharp.Files
{
    public class NavFileEdict
    {
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
    }
}
