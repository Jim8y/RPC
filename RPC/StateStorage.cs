using Neo.SmartContract.Framework.Services;

namespace RPC
{
    /// <summary>
    /// Security Requirement:
    ///     the string of each state should be consistent
    ///     make sure there is no typo issues.
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
