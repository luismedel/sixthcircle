using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    static class Op
    {
        public const int nop = 0x00;
        public const int alt = 0x01;
        public const int nbalt = 0x02;
        public const int @goto = 0x03;
        public const int call = 0x04;
        public const int frame = 0x05;
        public const int spawn = 0x06;
        public const int runt = 0x07;
        public const int load = 0x08;
        public const int mcall = 0x09;
        public const int mspawn = 0x0A;
        public const int mframe = 0x0B;
        public const int ret = 0x0C;
        public const int jmp = 0x0D;
        public const int @case = 0x0E;
        public const int exit = 0x0F;
        public const int @new = 0x10;
        public const int newa = 0x11;
        public const int newcb = 0x12;
        public const int newcw = 0x13;
        public const int newcf = 0x14;
        public const int newcp = 0x15;
        public const int newcm = 0x16;
        public const int newcmp = 0x17;
        public const int send = 0x18;
        public const int recv = 0x19;
        public const int consb = 0x1A;
        public const int consw = 0x1B;
        public const int consp = 0x1C;
        public const int consf = 0x1D;
        public const int consm = 0x1E;
        public const int consmp = 0x1F;
        public const int headb = 0x20;
        public const int headw = 0x21;
        public const int headp = 0x22;
        public const int headf = 0x23;
        public const int headm = 0x24;
        public const int headmp = 0x25;
        public const int tail = 0x26;
        public const int lea = 0x27;
        public const int indx = 0x28;
        public const int movp = 0x29;
        public const int movm = 0x2A;
        public const int movmp = 0x2B;
        public const int movb = 0x2C;
        public const int movw = 0x2D;
        public const int movf = 0x2E;
        public const int cvtbw = 0x2F;
        public const int cvtwb = 0x30;
        public const int cvtfw = 0x31;
        public const int cvtwf = 0x32;
        public const int cvtca = 0x33;
        public const int cvtac = 0x34;
        public const int cvtwc = 0x35;
        public const int cvtcw = 0x36;
        public const int cvtfc = 0x37;
        public const int cvtcf = 0x38;
        public const int addb = 0x39;
        public const int addw = 0x3A;
        public const int addf = 0x3B;
        public const int subb = 0x3C;
        public const int subw = 0x3D;
        public const int subf = 0x3E;
        public const int mulb = 0x3F;
        public const int mulw = 0x40;
        public const int mulf = 0x41;
        public const int divb = 0x42;
        public const int divw = 0x43;
        public const int divf = 0x44;
        public const int modw = 0x45;
        public const int modb = 0x46;
        public const int andb = 0x47;
        public const int andw = 0x48;
        public const int orb = 0x49;
        public const int orw = 0x4A;
        public const int xorb = 0x4B;
        public const int xorw = 0x4C;
        public const int shlb = 0x4D;
        public const int shlw = 0x4E;
        public const int shrb = 0x4F;
        public const int shrw = 0x50;
        public const int insc = 0x51;
        public const int indc = 0x52;
        public const int addc = 0x53;
        public const int lenc = 0x54;
        public const int lena = 0x55;
        public const int lenl = 0x56;
        public const int beqb = 0x57;
        public const int bneb = 0x58;
        public const int bltb = 0x59;
        public const int bleb = 0x5A;
        public const int bgtb = 0x5B;
        public const int bgeb = 0x5C;
        public const int beqw = 0x5D;
        public const int bnew = 0x5E;
        public const int bltw = 0x5F;
        public const int blew = 0x60;
        public const int bgtw = 0x61;
        public const int bgew = 0x62;
        public const int beqf = 0x63;
        public const int bnef = 0x64;
        public const int bltf = 0x65;
        public const int blef = 0x66;
        public const int bgtf = 0x67;
        public const int bgef = 0x68;
        public const int beqc = 0x69;
        public const int bnec = 0x6A;
        public const int bltc = 0x6B;
        public const int blec = 0x6C;
        public const int bgtc = 0x6D;
        public const int bgec = 0x6E;
        public const int slicea = 0x6F;
        public const int slicela = 0x70;
        public const int slicec = 0x71;
        public const int indw = 0x72;
        public const int indf = 0x73;
        public const int indb = 0x74;
        public const int negf = 0x75;
        public const int movl = 0x76;
        public const int addl = 0x77;
        public const int subl = 0x78;
        public const int divl = 0x79;
        public const int modl = 0x7A;
        public const int mull = 0x7B;
        public const int andl = 0x7C;
        public const int orl = 0x7D;
        public const int xorl = 0x7E;
        public const int shll = 0x7F;
        public const int shrl = 0x80;
        public const int bnel = 0x81;
        public const int bltl = 0x82;
        public const int blel = 0x83;
        public const int bgtl = 0x84;
        public const int bgel = 0x85;
        public const int beql = 0x86;
        public const int cvtlf = 0x87;
        public const int cvtfl = 0x88;
        public const int cvtlw = 0x89;
        public const int cvtwl = 0x8A;
        public const int cvtlc = 0x8B;
        public const int cvtcl = 0x8C;
        public const int headl = 0x8D;
        public const int consl = 0x8E;
        public const int newcl = 0x8F;
        public const int casec = 0x90;
        public const int indl = 0x91;
        public const int movpc = 0x92;
        public const int tcmp = 0x93;
        public const int mnewz = 0x94;
        public const int cvtrf = 0x95;
        public const int cvtfr = 0x96;
        public const int cvtws = 0x97;
        public const int cvtsw = 0x98;
        public const int lsrw = 0x99;
        public const int lsrl = 0x9A;
        public const int eclr = 0x9B;
        public const int newz = 0x9C;
        public const int newaz = 0x9D;
        public const int raise = 0x9E;

        public static Dictionary<int, string> NAMES = new Dictionary<int, string> {
            { Op.addb, "addb" },
            { Op.addc, "addc" },
            { Op.addf, "addf" },
            { Op.addl, "addl" },
            { Op.addw, "addw" },
            { Op.alt, "alt" },
            { Op.andb, "andb" },
            { Op.andl, "andl" },
            { Op.andw, "andw" },
            { Op.beqb, "beqb" },
            { Op.beqc, "beqc" },
            { Op.beqf, "beqf" },
            { Op.beql, "beql" },
            { Op.beqw, "beqw" },
            { Op.bgeb, "bgeb" },
            { Op.bgec, "bgec" },
            { Op.bgef, "bgef" },
            { Op.bgel, "bgel" },
            { Op.bgew, "bgew" },
            { Op.bgtb, "bgtb" },
            { Op.bgtc, "bgtc" },
            { Op.bgtf, "bgtf" },
            { Op.bgtl, "bgtl" },
            { Op.bgtw, "bgtw" },
            { Op.bleb, "bleb" },
            { Op.blec, "blec" },
            { Op.blef, "blef" },
            { Op.blel, "blel" },
            { Op.blew, "blew" },
            { Op.bltb, "bltb" },
            { Op.bltc, "bltc" },
            { Op.bltf, "bltf" },
            { Op.bltl, "bltl" },
            { Op.bltw, "bltw" },
            { Op.bneb, "bneb" },
            { Op.bnec, "bnec" },
            { Op.bnef, "bnef" },
            { Op.bnel, "bnel" },
            { Op.bnew, "bnew" },
            { Op.call, "call" },
            { Op.@case, "case" },
            { Op.casec, "casec" },
            { Op.consb, "consb" },
            { Op.consf, "consf" },
            { Op.consl, "consl" },
            { Op.consm, "consm" },
            { Op.consmp, "consmp" },
            { Op.consp, "consp" },
            { Op.consw, "consw" },
            { Op.cvtac, "cvtac" },
            { Op.cvtbw, "cvtbw" },
            { Op.cvtca, "cvtca" },
            { Op.cvtcf, "cvtcf" },
            { Op.cvtcl, "cvtcl" },
            { Op.cvtcw, "cvtcw" },
            { Op.cvtfc, "cvtfc" },
            { Op.cvtfl, "cvtfl" },
            { Op.cvtfr, "cvtfr" },
            { Op.cvtfw, "cvtfw" },
            { Op.cvtlc, "cvtlc" },
            { Op.cvtlf, "cvtlf" },
            { Op.cvtlw, "cvtlw" },
            { Op.cvtrf, "cvtrf" },
            { Op.cvtsw, "cvtsw" },
            { Op.cvtwb, "cvtwb" },
            { Op.cvtwc, "cvtwc" },
            { Op.cvtwf, "cvtwf" },
            { Op.cvtwl, "cvtwl" },
            { Op.cvtws, "cvtws" },
            { Op.divb, "divb" },
            { Op.divf, "divf" },
            { Op.divl, "divl" },
            { Op.divw, "divw" },
            { Op.eclr, "eclr" },
            { Op.exit, "exit" },
            { Op.frame, "frame" },
            { Op.@goto, "goto" },
            { Op.headb, "headb" },
            { Op.headf, "headf" },
            { Op.headl, "headl" },
            { Op.headm, "headm" },
            { Op.headmp, "headmp" },
            { Op.headp, "headp" },
            { Op.headw, "headw" },
            { Op.indb, "indb" },
            { Op.indc, "indc" },
            { Op.indf, "indf" },
            { Op.indl, "indl" },
            { Op.indw, "indw" },
            { Op.indx, "indx" },
            { Op.insc, "insc" },
            { Op.jmp, "jmp" },
            { Op.lea, "lea" },
            { Op.lena, "lena" },
            { Op.lenc, "lenc" },
            { Op.lenl, "lenl" },
            { Op.load, "load" },
            { Op.lsrl, "lsrl" },
            { Op.lsrw, "lsrw" },
            { Op.mcall, "mcall" },
            { Op.mframe, "mframe" },
            { Op.mnewz, "mnewz" },
            { Op.modb, "modb" },
            { Op.modl, "modl" },
            { Op.modw, "modw" },
            { Op.movb, "movb" },
            { Op.movf, "movf" },
            { Op.movl, "movl" },
            { Op.movm, "movm" },
            { Op.movmp, "movmp" },
            { Op.movp, "movp" },
            { Op.movpc, "movpc" },
            { Op.movw, "movw" },
            { Op.mspawn, "mspawn" },
            { Op.mulb, "mulb" },
            { Op.mulf, "mulf" },
            { Op.mull, "mull" },
            { Op.mulw, "mulw" },
            { Op.nbalt, "nbalt" },
            { Op.negf, "negf" },
            { Op.@new, "new" },
            { Op.newa, "newa" },
            { Op.newaz, "newaz" },
            { Op.newcb, "newcb" },
            { Op.newcf, "newcf" },
            { Op.newcl, "newcl" },
            { Op.newcm, "newcm" },
            { Op.newcmp, "newcmp" },
            { Op.newcp, "newcp" },
            { Op.newcw, "newcw" },
            { Op.newz, "newz" },
            { Op.nop, "nop" },
            { Op.orb, "orb" },
            { Op.orl, "orl" },
            { Op.orw, "orw" },
            { Op.recv, "recv" },
            { Op.ret, "ret" },
            { Op.runt, "runt" },
            { Op.send, "send" },
            { Op.shlb, "shlb" },
            { Op.shll, "shll" },
            { Op.shlw, "shlw" },
            { Op.shrb, "shrb" },
            { Op.shrl, "shrl" },
            { Op.shrw, "shrw" },
            { Op.slicea, "slicea" },
            { Op.slicec, "slicec" },
            { Op.slicela, "slicela" },
            { Op.spawn, "spawn" },
            { Op.subb, "subb" },
            { Op.subf, "subf" },
            { Op.subl, "subl" },
            { Op.subw, "subw" },
            { Op.tail, "tail" },
            { Op.tcmp, "tcmp" },
            { Op.xorb, "xorb" },
            { Op.xorl, "xorl" },
            { Op.xorw, "xorw" },
            { Op.raise, "raise" },
        };

        public static string GetName (int op)
        {
            string result;
            if (!NAMES.TryGetValue (op, out result))
                result = "0x" + op.ToString ("x");
            return result;
        }
    }
}