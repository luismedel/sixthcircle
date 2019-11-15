using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public class ObjectFile
    {
        const int XMAGIC = 819248;
        const int SMAGIC = 923426;

        const int FLAG_MUSTCOMPILE = 1 << 0;
        const int FLAG_DONTCOMPILE = 1 << 1;
        const int FLAG_SHAREMP = 1 << 2;

        public string Name { get; private set; }
        public Instruction[] Instructions { get; private set; }
        public TypeDescriptor[] Types { get; private set; }
        public DataDescriptor[] Data { get; private set; }
        public ExportDescriptor[] Link { get; private set; }
        public ModuleImportDescriptor[] Imports { get; private set; }
        public HandlerDescriptor[] Handlers { get; private set; }

        public bool ShareMP { get; private set; }
        public int StackExtent { get; private set; }

        public int EntryPC { get; private set; }
        public int EntryFrameType { get; private set; }

        public static ObjectFile FromFile (string path)
        {
            try
            {
                using (FileStream fs = File.OpenRead (path))
                {
                    DisReader reader = new DisReader (fs);
                    return FromReader (reader);
                }
            }
            catch (IOException)
            {
                return null;
            }
        }

        public static ObjectFile FromReader (DisReader reader)
        {
            int magic = reader.ReadOP ();

            if ((magic != XMAGIC) && (magic != SMAGIC))
                throw new InvalidDataException ("Invalid magic number");

            if (magic == SMAGIC)
            {
                int slength = reader.ReadOP ();
                byte[] signature = reader.ReadBytes (slength);
            }

            int runtimeFlag = reader.ReadOP ();
            int stackExtent = reader.ReadOP ();
            int codeSize = reader.ReadOP ();
            int dataSize = reader.ReadOP ();
            int typeSize = reader.ReadOP ();
            int linkSize = reader.ReadOP ();
            int entryPc = reader.ReadOP ();
            int entryType = reader.ReadOP ();

            ObjectFile result = new ObjectFile ();

            result.ShareMP = ((runtimeFlag & FLAG_SHAREMP) != 0);
            result.StackExtent = stackExtent;
            result.EntryPC = entryPc;
            result.EntryFrameType = entryType;
            
            result.Instructions = new Instruction[codeSize];
            for (int i = 0; i < codeSize; i++)
            {
                Instruction inst = Instruction.FromReader (reader);
                result.Instructions[i] = inst;
            }

            result.Types = new TypeDescriptor[typeSize];
            for (int i = 0; i < typeSize; i++)
                result.Types[i] = TypeDescriptor.FromReader (reader);


            List<DataDescriptor> data = new List<DataDescriptor> ();
            while (true)
            {
                DataDescriptor descriptor = DataDescriptor.FromReader (reader);
                if (descriptor.Code == 0)
                    break;

                data.Add (descriptor);
            }
            result.Data = data.ToArray ();

            result.Name = reader.ReadString ();

            result.Link = new ExportDescriptor[linkSize];
            for (int i = 0; i < linkSize; i++)
            {
                ExportDescriptor desc = ExportDescriptor.FromReader (reader);
                desc.FrameSize = result.Types[desc.FrameTypeId].Size;

                result.Link[i] = desc;
            }

            if (!reader.AtEndOfStream)
            {
                int importCount = reader.ReadOP ();

                List<ModuleImportDescriptor> imports = new List<ModuleImportDescriptor> (importCount);
                for (int i = 0; i < importCount; i++)
                    imports.Add (ModuleImportDescriptor.FromReader (reader));

                result.Imports = imports.ToArray ();
            }

            if (!reader.AtEndOfStream)
            {
                int handlerCount = reader.ReadOP ();

                List<HandlerDescriptor> handlers = new List<HandlerDescriptor> (handlerCount);
                for (int i = 0; i < handlerCount; i++)
                    handlers.Add (HandlerDescriptor.FromReader (reader));

                result.Handlers = handlers.ToArray ();
            }

            return result;
        }
    }
}
