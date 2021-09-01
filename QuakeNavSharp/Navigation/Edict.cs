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
        public class Edict
        {
            public Vector3 Mins { get; set; }
            public Vector3 Maxs { get; set; }

            public int Targetname { get; set; }
            public int Classname { get; set; }
        }
    }

    
}
