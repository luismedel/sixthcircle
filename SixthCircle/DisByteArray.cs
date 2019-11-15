using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public unsafe class DisByteArray: DisArray
    {
        byte[] _data;

        public byte[] InnerArray
        {
            get { return _data; }
        }

        public byte this[int index] { get { return _data[index]; } }

        public DisByteArray (int length)
            : base (length, 1)
        {
            _data = new byte[length];
        }

        public DisByteArray (byte[] data)
            : base (data.Length, 1)
        {
            _data = data;
        }

        public override IntPtr GetPointerAt (int position)
        {
            fixed (byte* ptr = &(_data[position]))
                return (IntPtr) ptr;
        }
    }
}
