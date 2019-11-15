using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public struct TypeDescriptor
    {
        public int Number;
        public int Size;
        public bool[] PointerMap;

        public bool OffsetIsPointer (int offset)
        {
            if (offset >= PointerMap.Length)
                return false;

            return PointerMap[offset];
        }

        public static TypeDescriptor FromReader (DisReader reader)
        {
            TypeDescriptor result = new TypeDescriptor ();

            result.Number = reader.ReadOP ();
            result.Size = reader.ReadOP ();

            int nbytes = reader.ReadOP ();
            result.PointerMap = new bool[nbytes * 8];

            if (nbytes != 0)
            {
                byte[] map = reader.ReadBytes (nbytes);
                for (int i = 0; i < nbytes * 8; i++)
                    result.PointerMap[i] = ((map[i / 8] & (1 << (i % 8))) != 0);
            }

            return result;
        }
    }
}