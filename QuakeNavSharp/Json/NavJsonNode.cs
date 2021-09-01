using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QuakeNavSharp.Json
{
    public class NavJsonNode
    {
        public Vector3 Origin { get; set; }
        public int Flags { get; set; }
        public NavJsonLink[] Links { get; set; }
        public int Radius { get; set; }
    }
}
