using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace RPC
{
    [ManifestExtra("Author", "Jinghui Liao")]
    [ManifestExtra("Email", "jinghui@wayne.edu")]
    [DisplayName("GAME-RPC")]
    [ManifestExtra("Description", "This is a rock-paper-scissors game to test the random number.")]
    [SupportedStandards("NEP-11")]
    [ContractPermission("*", "OnNEP11Payment")]
    [ContractTrust("0xd2a4cff31913016155e38e474a2c06d08be276cf")] // GAS contract
    public partial class RPC : SmartContract
    {
        //private enum Move
        //{
        //    Rock,
        //    Paper,
        //    Scissors
        //}

        [InitialValue("NdNXZuBvxSqhDAnk3AANxubDd5JNrB4d3a", ContractParameterType.Hash160)]
        static readonly UInt160 Owner = default;

        /// <summary>
        /// If the istrue is not true,
        /// then the transaction throw exception
        /// </summary>
        /// <param name="isTrue">true condition, has to be true to run</param>
        /// <param name="msg">Transaction FAULT reason</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Require(bool isTrue, string msg = "Invalid") { if (!isTrue) throw new Exception($"RPC::{msg}"); }

        [Safe]
        public static void CheckPaused() { Require(!StateStorage.IsPaused(), "paused"); }

        [Safe]
        public static UInt160 GetOwner()
        {
            var owner = OwnerMap.Get("owner");
            return owner != null ? (UInt160)owner : Owner;
        }

        [Safe]
        public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
        {
            CheckPaused();
            amount /= 100000000;
            var move = (byte)data;

            // I gonna check all parameters 
            // no matter what.
            Require(Runtime.CallingScriptHash == GAS.Hash);
            Require(Runtime.EntryScriptHash == GAS.Hash);
            Require(move == 0 || move == 1 || move == 2);
            Require(amount >= 1);
            Require(GAS.BalanceOf(Runtime.ExecutingScriptHash) >= amount);

            if (((Transaction)Runtime.ScriptContainer).Script.Length > 64)
                throw new Exception("RPC::Transaction script length error.");

            // if player wins, he gets 2 GAS
            if (PlayerWin(move))
                // The bigger you play, the more you get
                GAS.Transfer(Runtime.ExecutingScriptHash, from, 2 * amount);
        }

        /// <summary>
        /// Pick the winner
        /// return `true` if the player wins
        /// return `false` if the contract wins
        /// throw exception if draw
        /// </summary>
        /// <param name="move">the player more in the rage [0, 2]</param>
        /// <returns>is player wins </returns>
        private static bool PlayerWin(byte move)
        {
            var random = (byte)Runtime.GetRandom() % 3;
            switch (random - move)
            {
                case 1:
                case -2: return false;
                case 0: throw new Exception();
                case -1:
                default: return true;
            }
        }
    }
}