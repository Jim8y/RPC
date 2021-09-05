using Neo;
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
    [DisplayName("NEO-GAME-RPC")]
    [ManifestExtra("Description", "This is a rock-paper-scissors game to test the random number.")]
    [SupportedStandards("NEP-11")]
    [ContractPermission("*", "onNEP11Payment")]
    [ContractTrust("0xd2a4cff31913016155e38e474a2c06d08be276cf")] // GAS contract
    public partial class RPC : SmartContract
    {

        /// <summary>
        /// Security requirement:
        /// The prefix should be unique in the contract: checked globally.
        /// -- confirmed by jinghui
        /// </summary>
        private static readonly StorageMap PlayerMap = new(Storage.CurrentContext, (byte)StoragePrefix.Player);

        /// This fee suggestion makes sure
        /// that your transaction will not
        /// fail because of fee insufficiency.
        [Safe]
        public BigInteger GetFee() => 1_0000_0000; // temporary value, to be confirmed

        /// <summary>
        /// If the condition `istrue` does not hold,
        /// then the transaction throw exception
        /// making transaction `FAULT`
        /// </summary>
        /// <param name="isTrue">true condition, has to be true to run</param>
        /// <param name="msg">Transaction FAULT reason</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Require(bool isTrue, string msg = "Invalid") { if (!isTrue) throw new Exception($"RPC::{msg}"); }

        private static bool Paused() => StateStorage.IsPaused();

        /// <summary>
        /// Security requirements:
        /// <0> the amount has to be an 
        ///  positive integer greater 
        ///  than 1_0000_0000  but 
        ///  less than the amount the 
        ///  contract wins from the player: constrained internally
        ///  
        /// <1> the data should be one 
        /// byte of value among {0,1,2}: constrained internally
        /// 
        /// <2> transaction should be disabled
        /// if the contract is paused:  constrained internally
        /// 
        /// <3> The function call should not contain
        /// any post contract call and the entry 
        /// contract should by `GAS` : yet to confirm
        ///  
        /// </summary>
        /// <param name="from">the player address</param>
        /// <param name="amount">the amount of GAS the player bets</param>
        /// <param name="data">the move of the player</param>
        [Safe]
        public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
        {
            // <2> -- confirmed by jinghui
            Require(!Paused());

            // This is proposed by Chen Zhi Tong
            // If the player pays more than  the
            // amount he loses, he can always win
            // Since the contract can never win all the time.
            BigInteger earn = 0;
            var earnFrom = PlayerMap.Get(from);
            if (earnFrom is not null)
            {
                earn = (BigInteger)earnFrom;
                Require(earn >= amount);
            }

            var move = (byte)data;

            // I gonna check all parameters 
            // no matter what.
            // <3> -- yet to confirm
            Require(Runtime.CallingScriptHash == GAS.Hash);
            Require(Runtime.EntryScriptHash == ((Transaction)Runtime.ScriptContainer).Hash);
            if (((Transaction)Runtime.ScriptContainer).Script.Length > 90)
                throw new Exception("RPC::Transaction script length error.");

            // should not be called from a contract
            // --confirmed
            Require(ContractManagement.GetContract(from) is null);

            // <1> -- confirmed by jinghui
            Require(move == 0 || move == 1 || move == 2);

            // <0> -- confirmed by jinghui
            Require(amount >= 1_0000_0000);
            Require(GAS.BalanceOf(Runtime.ExecutingScriptHash) >= amount);

            // Check all possible conditions
            // --confirmed by jinghui
            if (PlayerWin(move))
            {
                // The bigger you play, the more you get
                GAS.Transfer(Runtime.ExecutingScriptHash, from, 2 * amount);
            }
            else
            {
                // If the game `draw`s, it won't even reach here.
                PlayerMap.Put(from, amount + earn);
            }
        }

        /// <summary>
        /// Pick the winner
        /// return `true` if the player wins
        /// return `false` if the contract wins
        /// throw exception if draw
        /// 
        /// Security requirements:
        /// <0> the move must be 
        ///     among {0, 1, 2} :  constrained by calling method
        ///     
        /// <1> the random must be 
        ///     evenly distributed
        ///     among {0, 1, 2} :   constrained by runtime function
        /// 
        /// </summary>
        /// <param name="move">the player more in the rage [0, 2]</param>
        /// <returns>is player wins </returns>
        private static bool PlayerWin(byte move)
        {
            // <1> Ensure that the random is evenly distributed
            // -- confirmed by jinghui
            var random = (byte)(Runtime.GetRandom() % 3);

            // Make sure all conditions are considered 
            // -- confirmed by jinghui
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

    /// <summary>
    /// Security requirement:
    ///     Each item has different value
    ///     -- confirmed by jinghui
    /// </summary>
    internal enum StoragePrefix
    {
        State = 0x14,
        Owner = 0x15,
        Player = 0x16,
    }

    //private enum Move
    //{
    //    Rock,
    //    Paper,
    //    Scissors
    //}
}