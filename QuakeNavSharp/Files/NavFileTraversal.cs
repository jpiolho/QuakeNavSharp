using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QuakeNavSharp.Files
{
    public class NavFileTraversal
    {
        /// <summary>
        /// First point in the traversal.
        /// </summary>
        public Vector3 Point1 { get; set; }

        /// <summary>
        /// Second point in the traversal.
        /// </summary>
        public Vector3 Point2 { get; set; }

        /// <summary>
        /// Third point in the traversal.
        /// </summary>
        public Vector3 Point3 { get; set; }
    }
}
