using System;
using System.IO;
using BTDB.Buffer;

namespace BTDB.SnappyCompression
{
    public static class SnappyDecompress
    {
        public static int DecompressedSize(Span<byte> compressedBytes, out int length)
        {
            var offset = 0;
            var limit = compressedBytes.Length;
            var b = compressedBytes[offset];
            offset++;
            var result = (uint)(b & 127);
            if (b < 128) goto done;
            if (offset >= limit) goto error;
            b = compressedBytes[offset];
            offset++;
            result |= ((uint)(b & 127) << 7);
            if (b < 128) goto done;
            if (offset >= limit) goto error;
            b = compressedBytes[offset];
            offset++;
            result |= ((uint)(b & 127) << 14);
            if (b < 128) goto done;
            if (offset >= limit) goto error;
            b = compressedBytes[offset];
            offset++;
            result |= ((uint)(b & 127) << 21);
            if (b < 128) goto done;
            if (offset >= limit) goto error;
            b = compressedBytes[offset];
            offset++;
            result |= ((uint)(b & 127) << 28);
            if (b >= 16) goto error;
        done:
            length = offset;
            return (int)result;
        error:
            length = 0;
            return -1;
        }

        public static ByteBuffer Decompress(ByteBuffer compressedBytes)
        {
            var decompressedSize = DecompressedSize(compressedBytes.ToSpan(), out var ofs);
            if (decompressedSize < 0) throw new InvalidDataException();
            var dst = new byte[decompressedSize];
            var dstBuf = ByteBuffer.NewAsync(dst);
            if (!DecompressRaw(dstBuf, ByteBuffer.NewSync(compressedBytes.Buffer, compressedBytes.Offset + ofs, compressedBytes.Length - ofs)))
                throw new InvalidDataException();
            return dstBuf;
        }

        public static Span<byte> Decompress(Span<byte> compressedBytes)
        {
            var decompressedSize = DecompressedSize(compressedBytes, out var ofs);
            if (decompressedSize < 0) throw new InvalidDataException();
            var dst = new Span<byte>(new byte[decompressedSize]);
            if (!DecompressRaw(dst, compressedBytes.Slice(ofs)))
                throw new InvalidDataException();
            return dst;
        }

        static bool DecompressRaw(Span<byte> dst, ReadOnlySpan<byte> src)
        {
            var s = 0;
            var d = 0;
            var sL = src.Length;
            var dL = dst.Length;
            int len = 0;
            int o = 0;
            while (sL > 0)
            {
                var b = src[s];
                s++;
                sL--;
                switch (b & 3)
                {
                    case 0:
                        len = b >> 2;
                        if (len < 60)
                        {
                            len++;
                        }
                        else if (len == 60)
                        {
                            if (sL < 1) return false;
                            len = src[s] + 1;
                            s++;
                            sL--;
                        }
                        else if (len == 61)
                        {
                            if (sL < 2) return false;
                            len = src[s] + 0x100 * src[s + 1] + 1;
                            s += 2;
                            sL -= 2;
                        }
                        else if (len == 62)
                        {
                            if (sL < 3) return false;
                            len = src[s] + 0x100 * src[s + 1] + 0x10000 * src[s + 2] + 1;
                            s += 3;
                            sL -= 3;
                        }
                        else
                        {
                            if (sL < 4) return false;
                            len = src[s] + 0x100 * src[s + 1] + 0x10000 * src[s + 2] + 0x1000000 * src[s + 3] + 1;
                            s += 3;
                            sL -= 3;
                        }
                        if (len <= 0) return false;
                        if (len > dL || len > sL) return false;
                        src.Slice(s, len).CopyTo(dst.Slice(d, len));
                        s += len;
                        d += len;
                        sL -= len;
                        dL -= len;
                        continue;
                    case 1:
                        if (sL < 1) return false;
                        len = 4 + ((b >> 2) & 7);
                        o = (b & 0xe0) << 3 | src[s];
                        s++;
                        sL--;
                        break;
                    case 2:
                        if (sL < 2) return false;
                        len = 1 + (b >> 2);
                        o = src[s] + src[s + 1] * 0x100;
                        s += 2;
                        sL -= 2;
                        break;
                    case 3:
                        return false;
                }
                var end = d + len;
                if (o > d || len > dL)
                    return false;
                for (; d < end; d++)
                {
                    dst[d] = dst[d - o];
                }
                dL -= len;
            }
            return dL == 0;
        }

        public static bool DecompressRaw(ByteBuffer dstBuf, ByteBuffer srcBuf)
        {
            var src = srcBuf.Buffer;
            var dst = dstBuf.Buffer;
            var s = srcBuf.Offset;
            var d = dstBuf.Offset;
            var sL = srcBuf.Length;
            var dL = dstBuf.Length;
            int len = 0;
            int o = 0;
            while (sL > 0)
            {
                var b = src[s];
                s++;
                sL--;
                switch (b & 3)
                {
                    case 0:
                        len = b >> 2;
                        if (len < 60)
                        {
                            len++;
                        }
                        else if (len == 60)
                        {
                            if (sL < 1) return false;
                            len = src[s] + 1;
                            s++;
                            sL--;
                        }
                        else if (len == 61)
                        {
                            if (sL < 2) return false;
                            len = src[s] + 0x100 * src[s + 1] + 1;
                            s += 2;
                            sL -= 2;
                        }
                        else if (len == 62)
                        {
                            if (sL < 3) return false;
                            len = src[s] + 0x100 * src[s + 1] + 0x10000 * src[s + 2] + 1;
                            s += 3;
                            sL -= 3;
                        }
                        else
                        {
                            if (sL < 4) return false;
                            len = src[s] + 0x100 * src[s + 1] + 0x10000 * src[s + 2] + 0x1000000 * src[s + 3] + 1;
                            s += 3;
                            sL -= 3;
                        }
                        if (len <= 0) return false;
                        if (len > dL || len > sL) return false;
                        Array.Copy(src, s, dst, d, len);
                        s += len;
                        d += len;
                        sL -= len;
                        dL -= len;
                        continue;
                    case 1:
                        if (sL < 1) return false;
                        len = 4 + ((b >> 2) & 7);
                        o = (b & 0xe0) << 3 | src[s];
                        s++;
                        sL--;
                        break;
                    case 2:
                        if (sL < 2) return false;
                        len = 1 + (b >> 2);
                        o = src[s] + src[s + 1] * 0x100;
                        s += 2;
                        sL -= 2;
                        break;
                    case 3:
                        return false;
                }
                var end = d + len;
                if (o > d || len > dL)
                    return false;
                for (; d < end; d++)
                {
                    dst[d] = dst[d - o];
                }
                dL -= len;
            }
            return dL == 0;
        }
    }
}
