using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public class NativeModule: IModule
    {
        public delegate void NativeMethod (Vm vm);

        class NativeMethodInfo
        {
            public string Name;
            public int Signature;
            public NativeMethod Method;
            public int FrameSize;
        }

        Vm _vm;
        Dictionary<string, NativeMethodInfo> _methods;

        public NativeModule (Vm vm)
        {
            _vm = vm;
            _methods = new Dictionary<string, NativeMethodInfo> ();
        }

        public virtual NativeMethod GetMethod (string name, int signature)
        {
            NativeMethodInfo result = null;
            if (!_methods.TryGetValue (HashMethod (name, signature), out result))
                return null;

            return result.Method;
        }

        public void RegisterMethod (string name, int signature, NativeMethod method, int frameSize)
        {
            _methods[HashMethod (name, signature)] = new NativeMethodInfo {
                Name = name,
                Signature = signature,
                FrameSize = frameSize,
                Method = method
            };
        }

        protected virtual string HashMethod (string name, int signature)
        {
            return name + "_0x" + signature.ToString ("x");
        }

        public virtual bool ValidateImports (ModuleImportDescriptor descriptor)
        {
            foreach (MethodImportDescriptor method in descriptor.Imports)
            {
                if (!_methods.ContainsKey (HashMethod (method.Name, method.Signature)))
                    return false;
            }

            return true;
        }

        public int GetFrameSize (string name, int signature)
        {
            return 40;

            NativeMethodInfo result = null;
            if (!_methods.TryGetValue (HashMethod (name, signature), out result))
                return 0;

            return result.FrameSize;
        }
    }
}
