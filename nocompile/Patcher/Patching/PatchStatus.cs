namespace Patcher.Patching
{
    /// <summary>
    ///     Represents a patch status, including whether the patch has been applied and if it was successful.
    /// </summary>
    /// <param name="Applied">Whether the patch has been applied.</param>
    /// <param name="Success">Whether the patch application was successful.</param>
    public readonly record struct PatchStatus(bool Applied, bool Success);
}