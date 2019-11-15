using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public struct DataDescriptor
    {
        static Dictionary<int, int> SIZES = new Dictionary<int, int> {
            { INT8, 1 },
            { INT32, 4 },
            { INT64, 8 },
            { FLOAT, 8 }
        };

        public const int INT8 = 0x01;
        public const int INT32 = 0x02;
        public const int INT64 = 0x08;
        public const int FLOAT = 0x04;
        public const int STRING = 0x03;

        public const int ARRAY = 0x05;
        public const int SET_ARRAY_ADDR = 0x06;
        public const int RESTORE_ARRAY_ADDR = 0x07;

        public int Code;
        public int Count;
        public int Offset;
        public byte[] Data;

        public int RequiredBytes
        {
            get
            {
                if (Code == STRING || Code == ARRAY)
                    return 4;

                return SIZES[Code];
            }
        }

        public static DataDescriptor FromReader (DisReader reader)
        {
            DataDescriptor result = new DataDescriptor { Code = 0 };

            int code = reader.ReadByte ();
            if (code == 0)
                return result;

            int count = code & 0x0f;
            if (count == 0)
                count = reader.ReadOP ();

            result.Code = (code & 0xf0) >> 4;
            result.Count = count;
            result.Offset = reader.ReadOP ();

            int dataSize = 0;
            if (result.Code == INT8)
                dataSize = 1;
            else if (result.Code == INT32)
                dataSize = 4;
            else if (result.Code == FLOAT)
                dataSize = 8;
            else if (result.Code == STRING)
                result.Data = Encoding.UTF8.GetBytes (reader.ReadString (count));

            if (dataSize != 0 && count != 0)
            {
                result.Data = reader.ReadBytes (count * dataSize);

                // big endian to little endian 
                result.Data = result.Data.Reverse ().ToArray ();
            }
            return result;
        }
    }
}
