using System.Numerics;

namespace QuakeNavSharp.Json
{
    public class NavJsonEdict
    {
        public Vector3 Mins { get; set; }
        public Vector3 Maxs { get; set; }
        public int Targetname { get; set; }
        public int Classname { get; set; }
    }
}