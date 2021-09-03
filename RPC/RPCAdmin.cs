using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Runtime.CompilerServices;

namespace RPC
{
    public partial class RPC
    {

        [InitialValue("NdNXZuBvxSqhDAnk3AANxubDd5JNrB4d3a", ContractParameterType.Hash160)]
        static readonly UInt160 Owner = default;

        private static readonly StorageMap OwnerMap = new(Storage.CurrentContext, 0x16);

        public static bool Verify() => Runtime.CheckWitness(GetOwner());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void OwnerOnly() { if (!Verify()) throw new Exception("No authorization."); }


        public static UInt160 SetOwner(UInt160 newOwner)
        {
            OwnerOnly();
            Require(newOwner.IsValid, "RPC::UInt160 is invalid.");
            OwnerMap.Put("owner", newOwner);
            return GetOwner();
        }

        [Safe]
        public static UInt160 GetOwner()
        {
            var owner = OwnerMap.Get("owner");
            return owner != null ? (UInt160)owner : Owner;
        }

        public static void _deploy(object _, bool update)
        {
            if (update) return;
        }

        public static void Update(ByteString nefFile, string manifest)
        {
            OwnerOnly();
            ContractManagement.Update(nefFile, manifest, null);
        }

        public static void Destroy()
        {
            OwnerOnly();
            ContractManagement.Destroy();
        }
        public static bool Pause()
        {
            OwnerOnly();
            StateStorage.Pause();
            return true;
        }

        public static bool Resume()
        {
            OwnerOnly();
            StateStorage.Resume();
            return true;
        }
    }
}
