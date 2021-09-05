// Copyright (C) 2021 Jinghui Liao.
// This file belongs to the NEO-GAME-RPC contract developed for neo N3
// Date: Sep-2-2021 
//
// The NEO-GAME-RPC is free smart contract distributed under the MIT software 
// license, see the accompanying file LICENSE in the main directory of
// the project or http://www.opensource.org/licenses/mit-license.php 
// for more details.
// 
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.SmartContract.Framework.Services;

namespace RPC
{
    /// <summary>
    /// Security Requirement:
    ///     the string of each state should be consistent,
    ///     making sure there is no typo issue.
    ///     -- confirmed by jinghui
    /// </summary>
    public static class StateStorage
    {
        /// <summary>
        /// Security requirement:
        ///     The prefix should be unique in the contract: checked globally.
        ///     -- confirmed by jinghui
        /// </summary>
        private static readonly StorageMap IndexMap = new(Storage.CurrentContext, (byte)StoragePrefix.State);

        private static readonly string key = "state";

        public static void Pause() => IndexMap.Put(key, "pause");

        public static void Resume() => IndexMap.Put(key, "");

        public static string GetState() => IndexMap.Get(key) == "pause" ? "pause" : "run";

        public static bool IsPaused() => GetState() == "pause";
    }
}
