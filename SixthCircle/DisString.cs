using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public class DisString: DisObject
    {
        public static readonly DisString Empty = new DisString ("");
        
        public byte[] Bytes { get; private set; }
        
        public string NativeValue
        {
            get { return Encoding.UTF8.GetString (Bytes); }
        }

        public DisString (byte[] data)
        {
            Bytes = data;
        }

        public DisString (string native)
            : this (Encoding.UTF8.GetBytes (native))
        {
        }

        public void InsertCharAt (int index, char chr)
        {
            string native = NativeValue;
            if (index >= native.Length)
                native += chr;
            else
                native = native.Substring (0, index - 1) +
                         chr +
                         native.Substring (index + 1);

            Bytes = Encoding.UTF8.GetBytes (native);
        }
    }
}
