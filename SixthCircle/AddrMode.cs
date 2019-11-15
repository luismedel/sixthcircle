using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    static class AddrMode
    {
        public const int MIDDLE_MASK = 0x03 << 6;
        public const int MIDDLE_NONE = 0x00;
        public const int MIDDLE_IMMEDIATE = 0x01 << 6;
        public const int MIDDLE_FPINDIRECT = 0x02 << 6;
        public const int MIDDLE_MPINDIRECT = 0x03 << 6;

        public const int SOURCE_MASK = 0x07 << 3;
        public const int SOURCE_MPINDIRECT = 0x00 << 3;
        public const int SOURCE_FPINDIRECT = 0x01 << 3;
        public const int SOURCE_IMMEDIATE = 0x02 << 3;
        public const int SOURCE_NONE = 0x03 << 3;
        public const int SOURCE_MPDOUBLEINDIRECT = 0x04 << 3;
        public const int SOURCE_FPDOUBLEINDIRECT = 0x05 << 3;

        public const int DESTINATION_MASK = 0x07;
        public const int DESTINATION_MPINDIRECT = 0x00;
        public const int DESTINATION_FPINDIRECT = 0x01;
        public const int DESTINATION_IMMEDIATE = 0x02;
        public const int DESTINATION_NONE = 0x03;
        public const int DESTINATION_MPDOUBLEINDIRECT = 0x04;
        public const int DESTINATION_FPDOUBLEINDIRECT = 0x05;
    }
}
