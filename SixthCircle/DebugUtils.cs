using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    static class DebugUtils
    {
        public static string DumpInstruction (Instruction inst)
        {
            string result = Op.GetName (inst.Opcode);
            string separator = "\t";

            int srcAddr = inst.AddressingMode & AddrMode.SOURCE_MASK;
            int destAddr = inst.AddressingMode & AddrMode.DESTINATION_MASK;
            int middleAddr = inst.AddressingMode & AddrMode.MIDDLE_MASK;

            if (srcAddr == AddrMode.SOURCE_IMMEDIATE)
                result += separator + "$" + inst.Source.ToString ();
            else if (srcAddr == AddrMode.SOURCE_MPINDIRECT)
                result += separator + inst.Source.ToString () + "(mp)";
            else if (srcAddr == AddrMode.SOURCE_FPINDIRECT)
                result += separator + inst.Source.ToString () + "(fp)";
            else if (srcAddr == AddrMode.SOURCE_MPDOUBLEINDIRECT)
                result += separator + (inst.Source & 0xff).ToString () + "(" + (inst.Source >> 16).ToString () + "(mp))";
            else if (srcAddr == AddrMode.SOURCE_FPDOUBLEINDIRECT)
                result += separator + (inst.Source & 0xff).ToString () + "(" + (inst.Source >> 16).ToString () + "(fp))";

            if (srcAddr != AddrMode.SOURCE_NONE)
                separator = ", ";

            if (middleAddr == AddrMode.MIDDLE_IMMEDIATE)
                result += separator + "$" + inst.Middle.ToString ();
            else if (middleAddr == AddrMode.MIDDLE_MPINDIRECT)
                result += separator + inst.Middle.ToString () + "(mp)";
            else if (middleAddr == AddrMode.MIDDLE_FPINDIRECT)
                result += separator + inst.Middle.ToString () + "(fp)";

            if (middleAddr != AddrMode.MIDDLE_NONE)
                separator = ", ";

            if (destAddr == AddrMode.DESTINATION_IMMEDIATE)
                result += separator + "$" + inst.Destination.ToString ();
            else if (destAddr == AddrMode.DESTINATION_MPINDIRECT)
                result += separator + inst.Destination.ToString () + "(mp)";
            else if (destAddr == AddrMode.DESTINATION_FPINDIRECT)
                result += separator + inst.Destination.ToString () + "(fp)";
            else if (destAddr == AddrMode.DESTINATION_MPDOUBLEINDIRECT)
                result += separator + (inst.Destination & 0xff).ToString () + "(" + (inst.Destination >> 16).ToString () + "(mp))";
            else if (destAddr == AddrMode.DESTINATION_FPDOUBLEINDIRECT)
                result += separator + (inst.Destination & 0xff).ToString () + "(" + (inst.Destination >> 16).ToString () + "(fp))";

            return result;
        }

        public static string DumpInstructions (ObjectFile obj)
        {
            return string.Join (Environment.NewLine, obj.Instructions
                                                        .Select (i => DumpInstruction (i))
                                                        .ToArray ());
        }
    }
}
