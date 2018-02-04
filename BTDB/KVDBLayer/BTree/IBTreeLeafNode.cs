using System;
using BTDB.Buffer;

namespace BTDB.KVDBLayer.BTree
{
    interface IBTreeLeafNode
    {
        ByteBuffer GetKey(int idx);
        Span<byte> GetKeyAsSpan(int idx);
        BTreeValue GetMemberValue(int idx);
        void SetMemberValue(int idx, BTreeValue value);
    }
}