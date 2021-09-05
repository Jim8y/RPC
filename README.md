# RPC

This is a rock-paper-scissors game developed for neo N3 and deployed on N3 testnet.

Contact address: 0x9c01a8640dff7c086dca99758d71645f57164d7c

## Usage

send at least 1 gas to 0x9c01a8640dff7c086dca99758d71645f57164d7c and attach an integer in the range [0,2] as move to the transaction.

## Core method

```C#
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
        /// <2> transaction should be FAULT
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
        public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
        {
            // <2> -- confirmed by jinghui
            Require(!Paused());

            if (from == GetOwner()) return;

            // This is proposed by Chen Zhi Tong
            // If the player pays more than the
            // amount he loses, he can always win
            // Since the contract can never win all the time.
            BigInteger earn = 0;
            var earnFrom = PlayerMap.Get(from);
            if (earnFrom is not null)
            {
                earn = (BigInteger)earnFrom;
                Require(earn >= amount, "You can not bet that much.");
            }

            var move = (byte)data;

            // I gonna check all parameters 
            // no matter what.
            // <3> -- yet to confirm
            Require(Runtime.CallingScriptHash == GAS.Hash, "Script format error.");
            //Require(Runtime.EntryScriptHash == GAS.Hash, "Runtime.EntryScriptHash == ((Transaction)Runtime.ScriptContainer).Hash");
            if (((Transaction)Runtime.ScriptContainer).Script.Length > 96)
                throw new Exception("RPC::Transaction script length error. No wapper contract or extra script allowed.");

            // should not be called from a contract
            // --confirmed
            Require(ContractManagement.GetContract(from) is null, "ContractManagement.GetContract(from) is null");

            // <1> -- confirmed by jinghui
            Require(move == 0 || move == 1 || move == 2, "Invalid move.");

            // <0> -- confirmed by jinghui
            Require(amount >= 1_0000_0000, "Please at least bet 1 GAS.");
            Require(GAS.BalanceOf(Runtime.ExecutingScriptHash) >= amount, "Insufficient balance");

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
```
