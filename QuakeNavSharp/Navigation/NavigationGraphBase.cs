using QuakeNavSharp.Files;
using QuakeNavSharp.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakeNavSharp.Navigation
{
    public abstract class NavigationGraphBase
    {
        public abstract NavFileBase ToNavFileGeneric();
        public abstract NavJsonBase ToNavJsonGeneric();
    }
}
