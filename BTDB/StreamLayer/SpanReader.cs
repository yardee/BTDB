using System;

namespace BTDB.StreamLayer
{
    public class SpanReader : AbstractBufferedReader, ICanMemorizePosition
    {

        public SpanReader(Span<byte> span)
        {
            Buf = span.ToArray(); //todo better
            End = Buf.Length;
        }

        protected override void FillBuffer()
        {
            Pos = -1;
            End = -1;
        }

        public override long GetCurrentPosition()
        {
            return Pos;
        }

        public IMemorizedPosition MemorizeCurrentPosition()
        {
            return new SpanReader.MemorizedPosition(this, Pos, End);
        }

        class MemorizedPosition : IMemorizedPosition
        {
            readonly SpanReader _owner;
            readonly int _pos;
            readonly int _end;

            internal MemorizedPosition(SpanReader owner, int pos, int end)
            {
                _owner = owner;
                _pos = pos;
                _end = end;
            }

            public void Restore()
            {
                _owner.Pos = _pos;
                _owner.End = _end;
            }
        }
    }
}