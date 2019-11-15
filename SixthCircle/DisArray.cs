using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public abstract class DisArray: DisObject
    {
        public int Length { get; private set; }
        public int ItemSize { get; private set; }

        public DisArray (int length, int itemSize)
        {
            this.Length = length;
            this.ItemSize = ItemSize;
        }

        public abstract IntPtr GetPointerAt (int position);
    }
}
