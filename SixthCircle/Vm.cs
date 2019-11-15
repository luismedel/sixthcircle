using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public class Vm
    {
        int objectCount = 0;

        Dictionary<string, IModule> _loadedModules;

        public byte[] Frame { get; private set; }
        public int FP { get; private set; }

        List<DisThread> _threads;
        DisThread _currentThread;
        
        Stack<CallFrame> _frames;

        public Dictionary<int, DisObject> Objects { get; private set; }
        Module _currentModule;

        Instruction _currentInstruction;

        public Vm ()
        {
            _loadedModules = new Dictionary<string, IModule> ();

            _frames = new Stack<CallFrame> ();
            _threads = new List<DisThread> ();

            Objects = new Dictionary<int, DisObject> (1024);
            Frame = new byte[1024];
            FP = 0;
        }

        IModule GetModule (string path, ModuleImportDescriptor descriptor)
        {
            IModule result;
            if (!_loadedModules.TryGetValue (path, out result))
            {
                ObjectFile obj = ObjectFile.FromFile (path);
                if (obj == null)
                    return null;

                result = Module.FromObjectFile (this, obj);
                _loadedModules[path] = result;
            }

            if (!result.ValidateImports (descriptor))
                return null;

            return result;
        }

        void PushCallFrame (int size)
        {
            PushCallFrame (new CallFrame {
                Module = _currentModule,
                FP = FP,
                Size = size,
                ReturnPC = _currentThread.PC
            });
        }

        void PushCallFrame (IntPtr newFP)
        {
            int size = Math.Abs (newFP.ToInt32 () - PointerToCurrentFrame ().ToInt32 ());

            PushCallFrame (size);
        }

        void PushCallFrame (CallFrame frame)
        {
            _frames.Push (frame);
        }

        void PopCallFrame ()
        {
            var f = _frames.Pop ();
            FP = f.FP;
            _currentThread.PC = f.ReturnPC;
            _currentModule = f.Module;
        }

        public void AddObject (DisObject obj)
        {
            obj.Id = ++objectCount;
            Objects[obj.Id] = obj;
        }

        public void StartMainThread (Module m)
        {
            if (m.Exports.Length == 0 || m.Exports[0].Name != "init")
                throw new Exception ("init method missing");

            TypeDescriptor type = m.Types[m.EntryFrameType];

            PushCallFrame (new CallFrame {
                Module = _currentModule,
                FP = 0,
                Size = type.Size,
                ReturnPC = -1
            });

            _threads.Add (new DisThread (m));
            FP = 0;
        }

        public bool Step ()
        {
            foreach (DisThread thread in _threads.Where (t => t.Active))
            {
                if ((thread.PC >= 0) && (thread.PC < thread.Module.Instructions.Length))
                {
                    _currentThread = thread;
                    _currentModule = thread.Module;
                    _currentInstruction = _currentModule.Instructions[thread.PC++];

                    ExecuteInstruction ();
                }
                else
                    thread.Active = false;
            }

            _threads.RemoveAll (t => !t.Active);

            return _threads.Count > 0;
        }

        unsafe IntPtr GetSourcePointer ()
        {
            int mode = _currentInstruction.AddressingMode & AddrMode.SOURCE_MASK;
            switch (mode)
            {
                case AddrMode.SOURCE_IMMEDIATE:
                    fixed (void* ptr = &(_currentInstruction.Source))
                        return new IntPtr (ptr);

                case AddrMode.SOURCE_MPINDIRECT:
                    fixed (void* ptr = _currentModule.MP)
                        return IntPtr.Add (new IntPtr (ptr), _currentInstruction.Source);

                case AddrMode.SOURCE_FPINDIRECT:
                    return IntPtr.Add (PointerToCurrentFrame (), _currentInstruction.Source);

                case AddrMode.SOURCE_MPDOUBLEINDIRECT:
                    fixed (void* ptr = _currentModule.MP)
                    {
                        IntPtr @base = IntPtr.Add (new IntPtr (ptr), _currentInstruction.Source >> 16);
                        IntPtr ind = new IntPtr (*(int*) @base.ToInt32 ());
                        return IntPtr.Add (ind, _currentInstruction.Source & 0xffff);
                    }
                
                case AddrMode.SOURCE_FPDOUBLEINDIRECT:
                    {
                        IntPtr @base = IntPtr.Add (PointerToCurrentFrame (), _currentInstruction.Source >> 16);
                        IntPtr ind = new IntPtr (*(int*) @base.ToInt32 ());
                        return IntPtr.Add (ind, _currentInstruction.Source & 0xffff);
                    }
            }

            return IntPtr.Zero;
        }

        unsafe IntPtr GetMiddlePointer ()
        {
            int mode = _currentInstruction.AddressingMode & AddrMode.MIDDLE_MASK;
            switch (mode)
            {
                case AddrMode.MIDDLE_NONE:
                    return GetDestinationPointer ();

                case AddrMode.MIDDLE_IMMEDIATE:
                    fixed (void* ptr = &(_currentInstruction.Middle))
                        return new IntPtr (ptr);

                case AddrMode.MIDDLE_MPINDIRECT:
                    fixed (void* ptr = _currentModule.MP)
                        return IntPtr.Add (new IntPtr (ptr), _currentInstruction.Middle);

                case AddrMode.MIDDLE_FPINDIRECT:
                    return IntPtr.Add (PointerToCurrentFrame (), _currentInstruction.Middle);
            }

            return IntPtr.Zero;
        }

        unsafe IntPtr GetDestinationPointer ()
        {
            int mode = _currentInstruction.AddressingMode & AddrMode.DESTINATION_MASK;
            switch (mode)
            {
                case AddrMode.DESTINATION_IMMEDIATE:
                    fixed (void* ptr = &(_currentInstruction.Destination))
                        return new IntPtr (ptr);

                case AddrMode.DESTINATION_MPINDIRECT:
                    fixed (void* ptr = _currentModule.MP)
                        return IntPtr.Add (new IntPtr (ptr), _currentInstruction.Destination);

                case AddrMode.DESTINATION_FPINDIRECT:
                    return IntPtr.Add (PointerToCurrentFrame (), _currentInstruction.Destination);

                case AddrMode.DESTINATION_MPDOUBLEINDIRECT:
                    fixed (void* ptr = _currentModule.MP)
                    {
                        IntPtr @base = IntPtr.Add (new IntPtr (ptr), _currentInstruction.Destination >> 16);
                        IntPtr ind = new IntPtr (*(int*) @base.ToInt32 ());
                        return IntPtr.Add (ind, _currentInstruction.Destination & 0xffff);
                    }

                case AddrMode.DESTINATION_FPDOUBLEINDIRECT:
                    {
                        IntPtr @base = IntPtr.Add (PointerToCurrentFrame (), _currentInstruction.Destination >> 16);
                        IntPtr ind = new IntPtr (* (int*) @base.ToInt32 ());
                        return IntPtr.Add (ind, _currentInstruction.Destination & 0xffff);
                    }
            }

            return IntPtr.Zero;
        }

        T ReadSourceAsObject<T> ()
            where T: DisObject
        {
            DisObject result = null;
            Objects.TryGetValue (ReadSourceAsInt32 (), out result);
            return (T) result;
        }

        unsafe byte ReadSourceAsByte ()
        {
            return * (byte*) GetSourcePointer ();
        }

        unsafe Int32 ReadSourceAsInt32 ()
        {
            return *(Int32*) GetSourcePointer ();
        }

        unsafe Int16 ReadSourceAsInt16 ()
        {
            return * (Int16*) GetSourcePointer ();
        }

        unsafe Int64 ReadSourceAsInt64 ()
        {
            return * (Int64*) GetSourcePointer ();
        }

        unsafe Double ReadSourceAsFloat ()
        {
            return * (Double*) GetSourcePointer ();
        }

        unsafe Single ReadSourceAsSingle ()
        {
            return * (Single*) GetSourcePointer ();
        }

        T ReadMiddleAsObject<T> ()
            where T: DisObject
        {
            DisObject result = null;
            Objects.TryGetValue (ReadMiddleAsInt32 (), out result);
            return (T) result;
        }

        unsafe byte ReadMiddleAsByte ()
        {
            return * (byte*) GetMiddlePointer ();
        }

        unsafe Int32 ReadMiddleAsInt32 ()
        {
            return *(Int32*) GetMiddlePointer ();
        }

        unsafe Int64 ReadMiddleAsInt64 ()
        {
            return * (Int64*) GetMiddlePointer ();
        }

        unsafe Double ReadMiddleAsFloat ()
        {
            return * (Double*) GetMiddlePointer ();
        }

        unsafe void WriteMiddleAsInt32 (Int32 value)
        {
            *((Int32*) GetMiddlePointer ()) = value;
        }

        T ReadDestinationAsObject<T> ()
            where T : DisObject
        {
            DisObject result = null;
            Objects.TryGetValue (ReadDestinationAsInt32 (), out result);
            return (T) result;
        }

        unsafe Int32 ReadDestinationAsInt32 ()
        {
            return *(Int32*) GetDestinationPointer ();
        }

        public void WriteDestinationAsObject (DisObject value)
        {
            if (value == null)
                WriteDestinationAsInt32 (0);
            else
            {
                if (value.Id == 0)
                    AddObject (value);

                WriteDestinationAsInt32 (value.Id);
            }
        }

        unsafe void WriteDestinationAsByte (byte value)
        {
            *((byte*) GetDestinationPointer ()) = value;
        }

        unsafe void WriteDestinationAsInt16 (Int16 value)
        {
            *((Int16*) GetDestinationPointer ()) = value;
        }

        unsafe void WriteDestinationAsInt32 (Int32 value)
        {
            *((Int32*) GetDestinationPointer ()) = value;
        }

        unsafe void WriteDestinationAsInt64 (Int64 value)
        {
            *((Int64*) GetDestinationPointer ()) = value;
        }

        unsafe void WriteDestinationAsFloat (Double value)
        {
            *((Double*) GetDestinationPointer ()) = value;
        }

        unsafe void WriteDestinationAsSingle (Single value)
        {
            *((Single*) GetDestinationPointer ()) = value;
        }

        public unsafe IntPtr PointerToFrame (int offset)
        {
            fixed (void* ptr = Frame)
                return IntPtr.Add (new IntPtr (ptr), offset);
        }

        public IntPtr PointerToCurrentFrame ()
        {
            return PointerToFrame (FP);
        }

        void ExecuteInstruction ()
        {
            switch (_currentInstruction.Opcode)
            {
                case Op.addb:
                    {
                        byte src = ReadSourceAsByte ();
                        byte middle = ReadMiddleAsByte ();
                        WriteDestinationAsByte ((byte) ((src + middle) & 0xff));
                        break;
                    }
                case Op.addc:
                    {
                        DisString src = ReadSourceAsObject<DisString> () ?? DisString.Empty;
                        DisString middle = ReadMiddleAsObject<DisString> () ?? DisString.Empty;
                        DisString result = new DisString (middle.NativeValue + src.NativeValue);
                        WriteDestinationAsObject (result);

                        break;
                    }
                case Op.addf:
                    {
                        double src = ReadSourceAsFloat ();
                        double middle = ReadMiddleAsFloat ();
                        WriteDestinationAsFloat (src + middle);
                        break;
                    }
                case Op.addl:
                    {
                        long src = ReadSourceAsInt64 ();
                        long middle = ReadMiddleAsInt64 ();
                        WriteDestinationAsInt64 (src + middle);
                        break;
                    }
                case Op.addw:
                    {
                        int src = ReadSourceAsInt32 ();
                        int middle = ReadMiddleAsInt32 ();
                        WriteDestinationAsInt32 (src + middle);
                        break;
                    }
                case Op.alt:
	                break;
                case Op.andb:
                    WriteDestinationAsByte ((byte) ((ReadSourceAsByte () & ReadMiddleAsByte ()) & 0xff));
	                break;
                case Op.andl:
                    WriteDestinationAsInt64 (ReadSourceAsInt64 () & ReadMiddleAsInt64 ());
	                break;
                case Op.andw:
                    WriteDestinationAsInt32 (ReadSourceAsInt32 () & ReadMiddleAsInt32 ());
	                break;
                case Op.beqb:
                    {
                        if (ReadSourceAsByte () == ReadMiddleAsByte ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.beqc:
	                break;
                case Op.beqf:
                    {
                        if (ReadSourceAsFloat () == ReadMiddleAsFloat ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.beql:
                    {
                        if (ReadSourceAsInt64 () == ReadMiddleAsInt64 ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.beqw:
                    {
                        if (ReadSourceAsInt32 () == ReadMiddleAsInt32 ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bgeb:
                    {
                        if (ReadSourceAsByte () >= ReadMiddleAsByte ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bgec:
	                break;
                case Op.bgef:
                    {
                        if (ReadSourceAsFloat () >= ReadMiddleAsFloat ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bgel:
                    {
                        if (ReadSourceAsInt64 () >= ReadMiddleAsInt64 ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bgew:
                    {
                        if (ReadSourceAsInt32 () >= ReadMiddleAsInt32 ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bgtb:
                    {
                        if (ReadSourceAsByte () > ReadMiddleAsByte ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bgtc:
	                break;
                case Op.bgtf:
                    {
                        if (ReadSourceAsFloat () > ReadMiddleAsFloat ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bgtl:
                    {
                        if (ReadSourceAsInt64 () > ReadMiddleAsInt64 ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bgtw:
                    {
                        if (ReadSourceAsInt32 () > ReadMiddleAsInt32 ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bleb:
                    {
                        if (ReadSourceAsByte () <= ReadMiddleAsByte ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.blec:
	                break;
                case Op.blef:
                    {
                        if (ReadSourceAsFloat () <= ReadMiddleAsFloat ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.blel:
                    {
                        if (ReadSourceAsInt64 () <= ReadMiddleAsInt64 ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.blew:
                    {
                        if (ReadSourceAsInt32 () <= ReadMiddleAsInt32 ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bltb:
                    {
                        if (ReadSourceAsByte () < ReadMiddleAsByte ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bltc:
	                break;
                case Op.bltf:
                    {
                        if (ReadSourceAsFloat () < ReadMiddleAsFloat ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bltl:
                    {
                        if (ReadSourceAsInt64 () < ReadMiddleAsInt64 ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bltw:
                    {
                        if (ReadSourceAsInt32 () < ReadMiddleAsInt32 ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bneb:
                    {
                        if (ReadSourceAsByte () != ReadMiddleAsByte ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bnec:
	                break;
                case Op.bnef:
                    {
                        if (ReadSourceAsFloat () != ReadMiddleAsFloat ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bnel:
                    {
                        if (ReadSourceAsInt64 () != ReadMiddleAsInt64 ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.bnew:
                    {
                        if (ReadSourceAsInt32 () != ReadMiddleAsInt32 ())
                            _currentThread.PC = ReadDestinationAsInt32 ();
                        break;
                    }
                case Op.call:
                    {
                        int currentSize = _frames.Peek ().Size;
                        PushCallFrame (new IntPtr (ReadSourceAsInt32 ()));
                        _currentThread.PC = ReadDestinationAsInt32 ();

                        int frame = PointerToCurrentFrame ().ToInt32 ();

                        FP = ReadSourceAsInt32 () - frame;
                        break;
                    }
                case Op.@case:
	                break;
                case Op.casec:
	                break;
                case Op.consb:
	                break;
                case Op.consf:
	                break;
                case Op.consl:
	                break;
                case Op.consm:
	                break;
                case Op.consmp:
	                break;
                case Op.consp:
	                break;
                case Op.consw:
	                break;
                case Op.cvtac:
                    {
                        DisByteArray array = ReadSourceAsObject <DisByteArray> ();
                        WriteDestinationAsObject (new DisString (array.InnerArray));
                        break;
                    }
                case Op.cvtbw:
                    WriteDestinationAsInt32 (ReadSourceAsByte ());
	                break;
                case Op.cvtca:
                    {
                        DisString str = ReadSourceAsObject<DisString> ();
                        WriteDestinationAsObject (new DisByteArray (str.Bytes));
                        break;
                    }
                case Op.cvtcf:
                    {
                        DisString str = ReadSourceAsObject<DisString> ();
                        WriteDestinationAsFloat (Double.Parse (str.NativeValue));
                        break;
                    }
                case Op.cvtcl:
                    {
                        DisString str = ReadSourceAsObject<DisString> ();
                        WriteDestinationAsInt64 (Int64.Parse (str.NativeValue));
                        break;
                    }
                case Op.cvtcw:
                    {
                        DisString str = ReadSourceAsObject<DisString> ();
                        WriteDestinationAsInt32 (Int32.Parse (str.NativeValue));
                        break;
                    }
                case Op.cvtfc:
                    WriteDestinationAsObject (new DisString (ReadSourceAsFloat ().ToString ()));
                    break;
                case Op.cvtfl:
                    WriteDestinationAsInt64 ((Int64) ReadSourceAsFloat ());
	                break;
                case Op.cvtfr:
                    WriteDestinationAsSingle ((Single) ReadSourceAsFloat ());
                    break;
                case Op.cvtfw:
                    WriteDestinationAsInt32 ((Int32) ReadSourceAsFloat ());
                    break;
                case Op.cvtlc:
                    WriteDestinationAsObject (new DisString (ReadSourceAsFloat ().ToString ()));
                    break;
                case Op.cvtlf:
                    WriteDestinationAsFloat (ReadSourceAsInt64 ());
	                break;
                case Op.cvtlw:
                    WriteDestinationAsInt32 ((int) ReadSourceAsInt64 ());
	                break;
                case Op.cvtrf:
                    WriteDestinationAsFloat ((Double) ReadSourceAsSingle ());
                    break;
                case Op.cvtsw:
                    WriteDestinationAsInt32 ((Int32) ReadSourceAsInt16 ());
                    break;
                case Op.cvtwb:
                    WriteDestinationAsByte ((byte) ReadSourceAsInt32 ());
	                break;
                case Op.cvtwc:
                    WriteDestinationAsObject (new DisString (ReadSourceAsInt32 ().ToString ()));
                    break;
                case Op.cvtwf:
                    WriteDestinationAsFloat (ReadSourceAsInt32 ());
	                break;
                case Op.cvtwl:
                    WriteDestinationAsInt64 (ReadSourceAsInt32 ());
	                break;
                case Op.cvtws:
                    WriteDestinationAsInt16 ((Int16) ReadSourceAsInt32 ());
	                break;
                case Op.divb:
                    WriteDestinationAsByte ((byte) (ReadMiddleAsByte () / ReadSourceAsByte ()));
	                break;
                case Op.divf:
                    WriteDestinationAsFloat (ReadMiddleAsFloat () / ReadSourceAsFloat ());
	                break;
                case Op.divl:
                    WriteDestinationAsInt64 (ReadMiddleAsInt64 () / ReadSourceAsInt64 ());
	                break;
                case Op.divw:
                    WriteDestinationAsInt32 (ReadMiddleAsInt32 () / ReadSourceAsInt32 ());
	                break;
                case Op.eclr:
	                break;
                case Op.exit:
	                break;
                case Op.frame:
                    {
                        int type = ReadSourceAsInt32 ();
                        IntPtr ptr = PointerToFrame (FP + _currentModule.Types[type].Size);
                        WriteDestinationAsInt32 (ptr.ToInt32 ());
                        break;
                    }
                case Op.@goto:
	                break;
                case Op.headb:
	                break;
                case Op.headf:
	                break;
                case Op.headl:
	                break;
                case Op.headm:
	                break;
                case Op.headmp:
	                break;
                case Op.headp:
	                break;
                case Op.headw:
	                break;
                case Op.indc:
                    {
                        DisString str = ReadSourceAsObject<DisString> ();
                        WriteDestinationAsInt32 ((int) str.NativeValue[ReadMiddleAsInt32 ()]);
                        break;
                    }
                case Op.indb:
                case Op.indf:
                case Op.indl:
                case Op.indw:
                case Op.indx:
                    {
                        DisArray array = ReadSourceAsObject<DisArray> ();
                        WriteMiddleAsInt32 ((int) array.GetPointerAt (ReadDestinationAsInt32 ()));
                        break;
                    }
                case Op.insc:
                    {
                        DisString str = ReadSourceAsObject<DisString> ();
                        str.InsertCharAt (ReadMiddleAsInt32 (), (char) ReadDestinationAsInt32 ());
                        break;
                    }
                case Op.jmp:
                    _currentThread.PC = ReadDestinationAsInt32 ();
	                break;
                case Op.lea:
                    {
                        WriteDestinationAsInt32 ((int) GetSourcePointer ());
                        break;
                    }
                case Op.lena:
                    {
                        DisArray array = ReadSourceAsObject<DisArray> ();
                        WriteDestinationAsInt32 (array.Length);
                        break;
                    }
                case Op.lenc:
                    {
                        DisString str = ReadSourceAsObject<DisString> () ?? DisString.Empty;
                        WriteDestinationAsInt32 (Encoding.UTF8.GetCharCount (str.Bytes));
                        break;
                    }
                case Op.lenl:
	                break;
                case Op.load:
                    {
                        string path = ReadSourceAsObject<DisString> ().NativeValue;

                        int linkId = ReadMiddleAsInt32 ();

                        ModuleImportDescriptor descriptor = _currentModule.ImportedModules[linkId];
                        IModule m = GetModule (path, descriptor);
                        if (m == null)
                            WriteDestinationAsObject (null);
                        else
                        {
                            DisModuleRef moduleRef = new DisModuleRef {
                                Module = m,
                                LinkId = linkId
                            };

                            WriteDestinationAsObject (moduleRef);
                        }
                        break;
                    }
                case Op.lsrl:
	                break;
                case Op.lsrw:
	                break;
                case Op.mcall:
                    {
                        PushCallFrame (new IntPtr (ReadSourceAsInt32 ()));

                        int currentSize = _frames.Peek ().Size;

                        int frame = PointerToCurrentFrame ().ToInt32 ();
                        FP = ReadSourceAsInt32 () - frame;

                        DisModuleRef moduleRef = ReadDestinationAsObject<DisModuleRef> ();
                        ModuleImportDescriptor mimp = _currentModule.ImportedModules[moduleRef.LinkId];
                        MethodImportDescriptor method = mimp.Imports[ReadMiddleAsInt32 ()];

                        if (moduleRef.Module is Module)
                        {
                            _currentModule = (Module) moduleRef.Module;
                            _currentThread.PC = _currentModule.Exports.FirstOrDefault (m => m.Signature == method.Signature && m.Name == method.Name).PC;
                        }
                        else
                        {
                            NativeModule native = (NativeModule) moduleRef.Module;
                            var m = native.GetMethod (method.Name, method.Signature);
                            m.Invoke (this);
                        }

                        break;
                    }
                case Op.mframe:
                    {
                        DisModuleRef moduleRef = ReadSourceAsObject<DisModuleRef> ();
                        ModuleImportDescriptor descriptor = _currentModule.ImportedModules[moduleRef.LinkId];
                        MethodImportDescriptor method = descriptor.Imports[ReadMiddleAsInt32 ()];

                        IntPtr ptr = PointerToFrame (FP + moduleRef.Module.GetFrameSize (method.Name, method.Signature));
                        WriteDestinationAsInt32 (ptr.ToInt32 ());
                        break;
                    }
                case Op.mnewz:
	                break;
                case Op.modb:
                    WriteDestinationAsByte ((byte) ((ReadMiddleAsByte () % ReadSourceAsByte ()) & 0xff));
	                break;
                case Op.modl:
                    WriteDestinationAsInt64 (ReadMiddleAsInt64 () % ReadSourceAsInt64 ());
	                break;
                case Op.modw:
                    WriteDestinationAsInt32 (ReadMiddleAsInt32 () % ReadSourceAsInt32 ());
	                break;
                case Op.movb:
                    WriteDestinationAsByte (ReadSourceAsByte ());
	                break;
                case Op.movf:
                    WriteDestinationAsFloat (ReadSourceAsFloat ());
	                break;
                case Op.movl:
                    WriteDestinationAsInt64 (ReadSourceAsInt64 ());
	                break;
                case Op.movm:
	                break;
                case Op.movmp:
	                break;
                case Op.movp:
                    {
                        WriteDestinationAsObject (ReadSourceAsObject<DisObject> ());
                        break;
                    }
                case Op.movpc:
	                break;
                case Op.movw:
                    {
                        WriteDestinationAsInt32 (ReadSourceAsInt32 ());
                        break;
                    }
                case Op.mspawn:
	                break;
                case Op.mulb:
                    WriteDestinationAsByte ((byte) ((ReadMiddleAsByte () * ReadSourceAsByte ()) & 0xff));
	                break;
                case Op.mulf:
                    WriteDestinationAsFloat (ReadMiddleAsFloat () * ReadSourceAsFloat ());
	                break;
                case Op.mull:
                    WriteDestinationAsInt64 (ReadMiddleAsInt64 () * ReadSourceAsInt64 ());
	                break;
                case Op.mulw:
                    WriteDestinationAsInt32 (ReadMiddleAsInt32 () * ReadSourceAsInt32 ());
	                break;
                case Op.nbalt:
	                break;
                case Op.negf:
                    WriteDestinationAsFloat (ReadSourceAsFloat () * -1.0);
	                break;
                case Op.@new:
	                break;
                case Op.newa:
                    {
                        int count = ReadSourceAsInt32 ();
                        int type = ReadMiddleAsInt32 ();
                        int size = _currentModule.Types[type].Size;
                        
                        DisArray array;
                        switch (size)
                        {
                            case 4:
                                array = new DisInt32Array (count);
                                break;
                            
                            case 8:
                                array = new DisInt64Array (count);
                                break;

                            default:
                                array = null;
                                break;
                        }

                        WriteDestinationAsObject (array);
                        break;
                    }
                case Op.newaz:
	                break;
                case Op.newcb:
	                break;
                case Op.newcf:
	                break;
                case Op.newcl:
	                break;
                case Op.newcm:
	                break;
                case Op.newcmp:
	                break;
                case Op.newcp:
	                break;
                case Op.newcw:
	                break;
                case Op.newz:
	                break;
                case Op.nop:
	                break;
                case Op.orb:
                    WriteDestinationAsByte ((byte) ((ReadSourceAsByte () * ReadMiddleAsByte()) & 0xff));
	                break;
                case Op.orl:
                    WriteDestinationAsInt64 (ReadSourceAsInt64 () * ReadMiddleAsInt64());
	                break;
                case Op.orw:
                    WriteDestinationAsInt32 (ReadSourceAsInt32 () * ReadMiddleAsInt32());
	                break;
                case Op.recv:
	                break;
                case Op.ret:
                    {
                        PopCallFrame ();
                        break;
                    }
                case Op.runt:
	                break;
                case Op.send:
	                break;
                case Op.shlb:
	                break;
                case Op.shll:
	                break;
                case Op.shlw:
	                break;
                case Op.shrb:
	                break;
                case Op.shrl:
	                break;
                case Op.shrw:
	                break;
                case Op.slicea:
	                break;
                case Op.slicec:
	                break;
                case Op.slicela:
	                break;
                case Op.spawn:
	                break;
                case Op.subb:
                    WriteDestinationAsByte ((byte) ((ReadMiddleAsByte () - ReadSourceAsByte ()) & 0xff));
	                break;
                case Op.subf:
                    WriteDestinationAsFloat (ReadMiddleAsFloat () - ReadSourceAsFloat ());
	                break;
                case Op.subl:
                    WriteDestinationAsInt64 (ReadMiddleAsInt64 () - ReadSourceAsInt64 ());
	                break;
                case Op.subw:
                    WriteDestinationAsInt32 (ReadMiddleAsInt32 () - ReadSourceAsInt32 ());
	                break;
                case Op.tail:
	                break;
                case Op.tcmp:
                    {
                        DisObject src = ReadSourceAsObject<DisObject> ();
                        DisObject dest = ReadDestinationAsObject<DisObject> ();
                        if (src.GetType () != dest.GetType ())
                            ; // error ("typecheck")
                        break;
                    }
                case Op.xorb:
                    WriteDestinationAsByte ((byte) ((ReadSourceAsByte () ^ ReadMiddleAsByte()) & 0xff));
	                break;
                case Op.xorl:
                    WriteDestinationAsInt64 (ReadSourceAsInt64 () ^ ReadMiddleAsInt64());
	                break;
                case Op.xorw:
                    WriteDestinationAsInt32 (ReadSourceAsInt32 () ^ ReadMiddleAsInt32());
	                break;
            }
        }

        public T ReadAsObject<T> (IntPtr ptr)
            where T: DisObject
        {
            DisObject result = null;
            Objects.TryGetValue (ReadAsInt32 (ptr), out result);
            return (T) result;
        }

        public unsafe Int32 ReadAsInt32 (IntPtr ptr)
        {
            return *(Int32*) ptr;
        }

        public void RegisterModule (string path, IModule module)
        {
            _loadedModules[path] = module;
        }
    }
}