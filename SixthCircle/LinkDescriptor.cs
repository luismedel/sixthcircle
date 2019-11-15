using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public class ExportDescriptor
    {
        public string Name { get; private set; }
        public int PC { get; private set; }
        public int FrameTypeId { get; private set; }
        public int Signature { get; private set; }
        public int FrameSize { get; set; }

        public static ExportDescriptor FromReader (DisReader reader)
        {
            ExportDescriptor result = new ExportDescriptor ();
            result.PC = reader.ReadOP ();
            result.FrameTypeId = reader.ReadOP ();
            result.Signature = reader.ReadInt32 ();
            result.Name = reader.ReadString ();

            return result;
        }
    }
}
