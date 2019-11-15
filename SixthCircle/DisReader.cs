using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public class DisReader
    {
        Stream _inner;

        public bool AtEndOfStream
        {
            get
            {
                bool result = _inner.ReadByte () == -1;
                _inner.Seek (-1, SeekOrigin.Current);
                return result;
            }
        }

        public DisReader (Stream stream)
        {
            _inner = stream;
        }

        public void AlignTo (int size)
        {
            long m = _inner.Position % (long) size;
            if (m == 0)
                return;

            _inner.Seek (m, SeekOrigin.Current);
        }

        public Int32 ReadOP ()
        {
            const int T_INT8 = 0x00 << 6;
            const int T_SINT8 = 0x01 << 6;
            const int T_INT16 = 0x02 << 6;
            const int T_INT32 = 0x03 << 6;
            const int T_MASK = 0x3 << 6;

            int first = ReadByte ();

            switch (first & T_MASK)
            {
                case T_INT8:
                    return first;

                case T_SINT8:
                    return first | ~0x7f;

                case T_INT16:
                    {
                        first = ((first & 0x20) == 0x20) ? (first | ~0x3F) : (first & 0x3F);
                        return (first << 8) | ReadByte ();
                    }

                case T_INT32:
                    {
                        first = ((first & 0x20) == 0x20) ? (first | ~0x3F) : (first & 0x3F);
                        return (first << 24) |
                               (ReadByte () << 16) |
                               (ReadByte () << 8) |
                               ReadByte ();
                    }
            }

            throw new InvalidDataException ();
        }

        public byte[] ReadBytes (int count)
        {
            byte[] result = new byte[count];

            if (_inner.Read (result, 0, count) != count)
                throw new EndOfStreamException ();

            return result;
        }

        public byte ReadByte ()
        {
            if (_inner.Position + 1 > _inner.Length)
                throw new EndOfStreamException ();

            return (byte) _inner.ReadByte ();
        }

        public Int16 ReadInt16 ()
        {
            if (_inner.Position + 2 > _inner.Length)
                throw new EndOfStreamException ();

            return (Int16) ((_inner.ReadByte () << 8) | _inner.ReadByte ());
        }

        public Int32 ReadInt32 ()
        {
            if (_inner.Position + 4 > _inner.Length)
                throw new EndOfStreamException ();

            return (_inner.ReadByte () << 24) |
                   (_inner.ReadByte () << 16) |
                   (_inner.ReadByte () << 8) |
                   _inner.ReadByte ();
        }

        public Int64 ReadInt64 ()
        {
            if (_inner.Position + 8 > _inner.Length)
                throw new EndOfStreamException ();

            return (_inner.ReadByte () << 56) |
                   (_inner.ReadByte () << 48) |
                   (_inner.ReadByte () << 40) |
                   (_inner.ReadByte () << 32) |
                   (_inner.ReadByte () << 24) |
                   (_inner.ReadByte () << 16) |
                   (_inner.ReadByte () << 8) |
                   _inner.ReadByte ();
        }

        public string ReadString ()
        {
            List<byte> bytes = new List<byte> (1024);

            byte b;
            while ((b = ReadByte ()) != 0)
                bytes.Add (b);

            return Encoding.UTF8.GetString (bytes.ToArray ());
        }

        public string ReadString (int count)
        {
            byte[] bytes = new byte[count];

            for (int i = 0; i < count; i++)
                bytes[i] = ReadByte ();

            return Encoding.UTF8.GetString (bytes.ToArray ());
        }
    }
}
