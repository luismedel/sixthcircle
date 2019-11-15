using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public struct Instruction
    {

        public byte Opcode;

        public int Source;
        public int Destination;
        public int Middle;

        public int AddressingMode;

        public static Instruction FromReader (DisReader reader)
        {
            Instruction result = new Instruction ();

            result.Opcode = reader.ReadByte ();
            result.AddressingMode = reader.ReadByte ();

            int srcAddr = result.AddressingMode & AddrMode.SOURCE_MASK;
            int destAddr = result.AddressingMode & AddrMode.DESTINATION_MASK;
            int middleAddr = result.AddressingMode & AddrMode.MIDDLE_MASK;

            if (middleAddr == AddrMode.MIDDLE_NONE)
                result.Middle = result.Destination;
            else
                result.Middle = reader.ReadOP ();

            if ((srcAddr == AddrMode.SOURCE_MPDOUBLEINDIRECT)
             || (srcAddr == AddrMode.SOURCE_FPDOUBLEINDIRECT))
                result.Source = (reader.ReadOP () << 16) | reader.ReadOP ();
            else if (srcAddr != AddrMode.SOURCE_NONE)
                result.Source = reader.ReadOP ();

            if ((destAddr == AddrMode.DESTINATION_MPDOUBLEINDIRECT)
             || (destAddr == AddrMode.DESTINATION_FPDOUBLEINDIRECT))
                result.Destination = (reader.ReadOP () << 16) | reader.ReadOP ();
            else if (destAddr != AddrMode.DESTINATION_NONE)
                result.Destination = reader.ReadOP ();

            return result;
        }
    }
}
