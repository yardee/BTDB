using System;

namespace BTDB.Buffer
{
    static class SpanManipulation
    {
        public static int CompareByteArray(Span<byte> s1, byte[] a2, int o2, int l2)
        {
            var commonLength = Math.Min(s1.Length, l2);
            for (var i = 0; i < commonLength; i++)
            {
                var b1 = s1[i];
                var b2 = a2[o2 + i];
                if (b1 == b2) continue;
                if (b1 < b2) return -1;
                return 1;
            }
            if (s1.Length == l2) return 0;
            if (s1.Length < l2) return -1;
            return 1;
        }
    }
}