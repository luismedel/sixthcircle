using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    class DisThread
    {
        public bool Active { get; set; }
        public int PC { get; set; }
        public byte[] MP { get; private set; }
        public Module Module { get; private set; }

        public DisThread (Module module)
        {
            this.Module = module;
            MP = new byte[1024];
            Active = true;

            PC = module.EntryPC;
        }
    }
}
