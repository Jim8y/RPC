using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Runtime.CompilerServices;

namespace RPC
{
    public partial class RPC
    {
        private static readonly StorageMap OwnerMap = new(Storage.CurrentContext, 0x16);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void OwnerOnly() { if (!Runtime.CheckWitness(GetOwner())) throw new Exception("No authorization."); }


        public static UInt160 SetOwner(UInt160 newOwner)
        {
            OwnerOnly();
            Require(!newOwner.IsValid, "RPC::SetOwner: UInt160 is invalid.");
            OwnerMap.Put("owner", newOwner);
            return GetOwner();
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
