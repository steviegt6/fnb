namespace Patcher.API.Patching.Loading
{
    /// <summary>
    /// A weighted loadable type.
    /// </summary>
    public interface IPatcherLoadable
    {
        /// <summary>
        ///     The weight of this loadable, ranging between 0 and 1, where 0 is the lowest priority and 1 is the highest.
        /// </summary>
        float Weight => 0.5f;

        /// <summary>
        ///     Code to run once this loadable is loaded.
        /// </summary>
        void Load();

        /// <summary>
        ///     Code to run once this loadable is unloaded.
        /// </summary>
        void Unload();
    }
}