using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.ComponentModel;
using System.Numerics;

namespace RPC
{
    [DisplayName("NFT-RPC")]
    [SupportedStandards("NEP-11")]
    [ContractPermission("*", "OnNEP11Payment")]
    [ContractTrust("0xd2a4cff31913016155e38e474a2c06d08be276cf")]
    public partial class RPC : Nep11Token<Nep11TokenState>
    {
        private enum CardType
        {
            N,
            E,
            O
        }


        [InitialValue("NSuyiLqEfEQZsLJawCbmXW154StRUwdWoM", ContractParameterType.Hash160)]
        static readonly UInt160 Owner = default;

        private static readonly int[] CardTypeMaxNum = { 100, 300, 3000 };

        internal static readonly int[] CardTypeInitialNum = { 0, 100, 300 };

        [Safe]
        public override string Symbol() => "RPC";

        [Safe]
        public static bool IsPaused() => StateStorage.IsPaused();

        [Safe]
        public static UInt160 GetOwner()
        {
            var owner = OwnerMap.Get("owner");
            return owner != null ? (UInt160)owner : Owner;
        }

        public static void OnNEP17Payment(UInt160 from, BigInteger amount, object _)
        {
            if (IsPaused()) throw new Exception("RPC::OnNEP17Payment: Suspension of sale.");

            amount /= 100000000;

            if (Runtime.CallingScriptHash != GAS.Hash || amount % 2 != 0)
                throw new Exception("RPC::OnNEP17Payment: The amount must be an integer multiple of 2GAS.");

           
        }

        public static bool UnBoxing(ByteString tokenId)
        {
            if (Runtime.EntryScriptHash != Runtime.CallingScriptHash) throw new Exception("RPC::UnBoxing: Contract calls are not allowed.");
            if (((Transaction)Runtime.ScriptContainer).Script.Length > 58) throw new Exception("RPC::UnBoxing: Transaction script length error.");

            return UnBoxingInternal(tokenId);
        }


    }
}

