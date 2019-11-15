using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    class CallFrame
    {
        public Module Module { get; set; }
        public int FP { get; set; }
        public int Size { get; set; }
        public int ReturnPC { get; set; }
    }
}
