namespace TML.Files.Specific.Data
{
    /// <summary>
    ///     Mod side enum. Replicates the tModLoader enum of the same name.
    /// </summary>
    public enum ModSide
    {
        /// <summary>
        ///     Synced across both the client and server.
        /// </summary>
        Both,

        /// <summary>
        ///     Client-side mod, no impact on other players or the server.
        /// </summary>
        Client,

        /// <summary>
        ///     Server-side mod, does not do anything that requires a mod on the player's side.
        /// </summary>
        Server,
        
        /// <summary>
        ///     Do not sync.
        /// </summary>
        NoSync
    }
}