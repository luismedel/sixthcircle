using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public unsafe class DisInt64Array: DisArray
    {
        Int64[] _data;

        public Int64 this[int index] { get { return _data[index]; } }

        public DisInt64Array (int length)
            : base (length, 8)
        {
            _data = new Int64[length];
        }

        public override IntPtr GetPointerAt (int position)
        {
            fixed (Int64* ptr = &(_data[position]))
                return (IntPtr) ptr;
        }
    }
}
