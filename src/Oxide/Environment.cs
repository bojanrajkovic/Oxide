using static Oxide.Options;

namespace Oxide
{
    /// <summary>
    ///     Environment helpers.
    /// </summary>
    public static class Environment
    {
        /// <summary>
        ///     Gets an <see cref="Option{TOption}" /> for the value of the given environment variable.
        /// </summary>
        /// <param name="environmentVariable">The environment variable name.</param>
        /// <returns>
        ///     <see cref="Some{T}" /> with the value of the environment variable, if there is a value,
        ///     <see cref="None{T}" /> otherwise.
        /// </returns>
        public static Option<string> GetEnvironmentVariable(string environmentVariable)
        {
            try {
                var value = System.Environment.GetEnvironmentVariable(environmentVariable);
                return string.IsNullOrWhiteSpace(value) ? None<string>() : Some(value);
            } catch {
                return None<string>();
            }
        }
    }
}
