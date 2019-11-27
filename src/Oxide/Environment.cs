using static Oxide.Options;

namespace Oxide
{
    class Environment
    {
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
