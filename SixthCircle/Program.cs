using SixthCircle.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    class Program
    {
        static void Main (string[] args)
        {
            Vm vm = new Vm ();
            vm.RegisterModule ("$Sys", new SysModule (vm));

            ObjectFile f = ObjectFile.FromFile (@"C:\inferno\test.dis");
            Module m = Module.FromObjectFile (vm, f);

            vm.StartMainThread (m);
            while (vm.Step ())
                ;
        }
    }
}
