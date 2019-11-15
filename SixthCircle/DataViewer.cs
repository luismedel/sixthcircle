using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public unsafe class DataViewer
    {
        byte[] _data;

        public DataViewer (byte[] data)
        {
            _data = data;
        }

        public Int16 AsInt16 (int offset)
        {
            fixed (void* ptr = &_data[offset])
                return *(Int16*) ptr;
        }

        public Int32 AsInt32 (int offset)
        {
            fixed (void* ptr = &_data[offset])
                return *(Int32*) ptr;
        }

        public Int64 AsInt64 (int offset)
        {
            fixed (void* ptr = &_data[offset])
                return *(Int64*) ptr;
        }

        public Single AsSingle (int offset)
        {
            fixed (void* ptr = &_data[offset])
                return *(Single*) ptr;
        }

        public Double AsDouble (int offset)
        {
            fixed (void* ptr = &_data[offset])
                return *(Double*) ptr;
        }
    }
}
