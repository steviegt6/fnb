using log4net;

namespace Patcher.Logging
{
    /// <summary>
    ///     An extensive wrapper around an <see cref="ILog"/> instance.
    /// </summary>
    public readonly struct LogWrapper
    {
        /// <summary>
        ///     The wrapped <see cref="ILog"/> instance.
        /// </summary>
        public readonly ILog Logger;

        /// <summary>
        ///     Creates a new <see cref="LogWrapper"/> instance.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> instance to wrap.</param>
        public LogWrapper(ILog logger)
        {
            Logger = logger;
        }

        /// <summary>
        ///     Boilerplate for patch logging.
        /// </summary>
        /// <param name="type">The patch failure type.</param>
        /// <param name="message">The message to log.</param>
        public void LogPatchFailure(string type, string message) => Logger.Error($"PATCH FAILURE {type} @ " + message);

        /// <summary>
        ///     Logs an op-code jump failure.
        /// </summary>
        /// <param name="typeName">The name of the patched type.</param>
        /// <param name="typeMethod">The name of the patched method.</param>
        /// <param name="opcode">The opcode which could not be jumped to.</param>
        /// <param name="value"></param>
        public void LogOpCodeJumpFailure(string typeName, string typeMethod, string opcode, string? value) =>
            LogPatchFailure(
                "OpCode Jump Failure",
                $"{typeName}::{typeMethod} -> {opcode}{(value is not null ? $" {value}" : "")}"
            );
    }
}