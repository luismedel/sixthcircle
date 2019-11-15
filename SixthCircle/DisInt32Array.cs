using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public unsafe class DisInt32Array: DisArray
    {
        int[] _data;

        public int this[int index] { get { return _data[index]; } }

        public DisInt32Array (int length)
            : base (length, 4)
        {
            _data = new int[length];
        }

        public override IntPtr GetPointerAt (int position)
        {
            fixed (int* ptr = &(_data[position]))
                return (IntPtr) ptr;
        }
    }
}
