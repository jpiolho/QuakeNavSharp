using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QuakeNavSharp.Navigation
{
    public sealed partial class NavigationGraph
    {
        public class Traversal
        {
            public Vector3 Point1 { get; set; }
            public Vector3 Point2 { get; set; }
            public Vector3 Point3 { get; set; }
        }
    }
}
