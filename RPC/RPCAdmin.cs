using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Runtime.CompilerServices;

namespace RPC
{
    /// <summary>
    /// Security Requirement:
    ///  All public functions in this partial class
    ///  that has write permission must be owner only
    ///  
    ///  [SetOwner] -- confirmed by jinghui
    ///  [_deploy]  -- except this one, confirmed by jinghui
    ///  [Update]   -- confirmed by jinghui
    ///  [Destroy]  -- confirmed by jinghui
    ///  [Pause]    -- confirmed by jinghui
    ///  [Resume]   -- confirmed by jinghui
    ///  
    /// </summary>
    public partial class RPC
    {

        [InitialValue("NaA5nQieb5YGg5nSFjhJMVEXQCQ5HdukwP", ContractParameterType.Hash160)]
        static readonly UInt160 Owner = default;

        /// <summary>
        /// Security requirement:
        /// The prefix should be unique in the contract: checked globally.
        /// </summary>
        private static readonly StorageMap OwnerMap = new(Storage.CurrentContext, (byte)StoragePrefix.Owner);

        public static bool Verify() => Runtime.CheckWitness(GetOwner());


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void OwnerOnly() { if (!Verify()) throw new Exception("No authorization."); }


        /// <summary>
        /// Security Requirements:
        /// <0> Only the owner of the contract
        /// are allowed to call this function: constrained internally
        /// 
        /// <1> the new address should be 
        /// a valid address: constrained internally
        /// 
        /// </summary>
        /// <param name="newOwner"></param>
        /// <returns></returns>
        public static UInt160 SetOwner(UInt160 newOwner)
        {
            // <0> -- confirmed by jinghui
            OwnerOnly();
            // <1> -- confirmed by jinghui
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
