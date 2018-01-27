using System;

namespace BTDB.StreamLayer
{
    public class SpanWriter : AbstractBufferedWriter
    {
        public SpanWriter()
        {
            Buf = new byte[32];
            End = Buf.Length;
        }

        public override void FlushBuffer()
        {
            var newLen = Math.Max((int)Math.Min((long)End * 2, 2147483591), 128);
            if (newLen == End) throw new OutOfMemoryException();
            Array.Resize(ref Buf, newLen);
            End = Buf.Length;
        }

        public Span<byte> GetDataAndRewind()
        {
            var result = Data;
            Buf = new byte[32];
            End = Buf.Length;
            Pos = 0;
            return result;
        }

        public override long GetCurrentPosition()
        {
            return Pos;
        }

        public Span<byte> Data
        {
            get
            {
                if (Pos == 0)
                    return Span<byte>.Empty;
                return new Span<byte>(Buf, 0, Pos);
            }
        }
    }
}