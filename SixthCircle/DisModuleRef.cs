using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public class DisModuleRef: DisObject
    {
        public IModule Module { get; set; }
        public int LinkId { get; set; }
    }
}
