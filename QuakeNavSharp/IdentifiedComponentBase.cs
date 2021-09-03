using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakeNavSharp.Navigation
{
    public abstract class IdentifiedComponentBase
    {
        public int Id { get; internal set; }
    }
}
