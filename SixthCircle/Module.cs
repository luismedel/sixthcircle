using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public class Module : IModule
    {
        static byte[] _sharedMP;

        public Vm Vm { get; private set; }
        
        public TypeDescriptor[] Types { get; private set; }
        public Instruction[] Instructions { get; private set; }
        public ExportDescriptor[] Exports { get; private set; }
        public ModuleImportDescriptor[] ImportedModules { get; private set; }
        public byte[] MP { get; private set; }

        public int EntryPC { get; private set; }
        public int EntryFrameType { get; private set; }

        Module (Vm vm)
        {
            this.Vm = vm;
        }

        public bool ValidateImports (ModuleImportDescriptor descriptor)
        {
            foreach (MethodImportDescriptor method in descriptor.Imports)
            {
                if (Exports.FirstOrDefault (lnk => lnk.Signature == method.Signature && lnk.Name == method.Name) == null)
                    return false;
            }

            return true;
        }

        public static Module FromObjectFile (Vm vm, ObjectFile file)
        {
            Module result = new Module (vm);
            result.Types = file.Types;
            result.Instructions = file.Instructions;

            if (file.ShareMP)
            {
                if (_sharedMP == null)
                    _sharedMP = ExpandData (vm, file.Data);
                result.MP = _sharedMP;
            }
            else
                result.MP = ExpandData (vm, file.Data);

            result.Exports = file.Link;
            result.ImportedModules = file.Imports;

            result.EntryPC = file.EntryPC;
            result.EntryFrameType = file.EntryFrameType;

            return result;
        }

        static unsafe byte[] ExpandData (Vm vm, DataDescriptor[] data)
        {
            if (data.Length == 0)
                return new byte[4];

            int size = data.Max (d => d.Offset + d.RequiredBytes);

            byte[] result = new byte[size];

            fixed (byte* mp = result)
            {
                foreach (DataDescriptor desc in data)
                {
                    int* ptr = (int*) (mp + desc.Offset);
                    if (desc.Code == DataDescriptor.STRING)
                        *ptr = CreateString (vm, desc.Data);
                    else if (desc.Code == DataDescriptor.ARRAY)
                        ;
                    else
                        Marshal.Copy (desc.Data, 0, (IntPtr) ptr, desc.Data.Length);
                }
            }

            return result;
        }

        static Int32 CreateString (Vm vm, byte[] data)
        {
            DisString str = new DisString (data);
            vm.AddObject (str);
            return str.Id;
        }

        public int GetFrameSize (string name, int signature)
        {
            ExportDescriptor link = Exports.FirstOrDefault (e => e.Signature == signature && e.Name == name);
            if (link == null)
                return 0;

            return link.FrameSize;
        }
    }
}
