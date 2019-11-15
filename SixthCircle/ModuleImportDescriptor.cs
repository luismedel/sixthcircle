using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public class ModuleImportDescriptor
    {
        public MethodImportDescriptor[] Imports { get; private set; }

        public static ModuleImportDescriptor FromReader (DisReader reader)
        {
            int size = reader.ReadOP ();

            List<MethodImportDescriptor> imports = new List<MethodImportDescriptor> ();
            for (int i = 0; i < size; i++)
                imports.Add (MethodImportDescriptor.FromReader (reader));

            return new ModuleImportDescriptor {
                Imports = imports.ToArray ()
            };
        }
    }
}
