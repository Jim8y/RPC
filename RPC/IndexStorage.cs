using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;

namespace RPC
{
    public static class IndexStorage
    {
        private static readonly StorageMap IndexMap = new(Storage.CurrentContext, 0x14);

        public static BigInteger CurrentIndex(byte type)
        {
            if (type < 0 || type > 3) throw new Exception("The argument \"type\" is invalid");
            return (BigInteger)IndexMap.Get(type.ToString());
        }

        public static BigInteger NextIndex(byte type)
        {
            var value = CurrentIndex(type) + 1;
            IndexMap.Put(type.ToString(), value);
            return value;
        }

        public static void Initial()
        {
        }
    }
}
