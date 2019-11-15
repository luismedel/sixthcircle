using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public class MethodImportDescriptor
    {
        public int Signature { get; private set; }
        public string Name { get; private set; }

        MethodImportDescriptor ()
        { }

        public static MethodImportDescriptor FromReader (DisReader reader)
        {
            return new MethodImportDescriptor {
                Signature = reader.ReadInt32 (),
                Name = reader.ReadString ()
            };
        }
    }
}
