using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public interface IModule
    {
        int GetFrameSize (string name, int signature);
        bool ValidateImports (ModuleImportDescriptor descriptor);
    }
}
