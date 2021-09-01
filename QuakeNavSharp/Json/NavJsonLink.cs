using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QuakeNavSharp.Json
{
    public class NavJsonLink
    {
        public Vector3 Target { get; set; }
        public int Type { get; set; }
        public Vector3[] Traversal { get; set; }
        public NavJsonEdict Edict { get; set; }
    }
}
