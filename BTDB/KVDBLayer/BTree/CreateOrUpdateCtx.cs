using System;
using System.Collections.Generic;

namespace BTDB.KVDBLayer.BTree
{
    struct CreateOrUpdateCtx
    {
        internal byte[] KeyPrefix;
        internal Span<byte> Key;
        internal uint ValueFileId;
        internal uint ValueOfs;
        internal int ValueSize;

        internal bool Created;
        internal List<NodeIdxPair> Stack;
        internal long KeyIndex;

        internal int Depth;
        internal long TransactionId;
        internal bool Split; // Node1+Node2 set
        internal bool SplitInRight; // false key is in Node1, true key is in Node2
        internal bool Update; // Node1 set
        internal IBTreeNode Node1;
        internal IBTreeNode Node2;

        internal int WholeKeyLen => KeyPrefix.Length + Key.Length;

        internal byte[] WholeKey()
        {
            if (KeyPrefix.Length == 0)
            {
                return Key.ToArray();
            }
            var result = new byte[KeyPrefix.Length + Key.Length];
            Array.Copy(KeyPrefix, result, KeyPrefix.Length);
            Key.CopyTo(new Span<byte>(result, KeyPrefix.Length, Key.Length));
            return result;
        }
    }
}