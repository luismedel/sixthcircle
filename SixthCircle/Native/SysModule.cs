using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle.Native
{
    public class SysModule: NativeModule
    {
        public SysModule (Vm vm)
            : base (vm)
        {
            RegisterMethod ("bind", 0, fn_bind, 40);
        }

        void fn_bind (Vm vm)
        {
        }

        protected override string HashMethod (string name, int signature)
        {
            return name;
        }

        public override bool ValidateImports (ModuleImportDescriptor descriptor)
        {
            return true;
        }
    }
}
