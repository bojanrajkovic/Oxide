using static Oxide.Options;

#if NETSTANDARD1_3

namespace Oxide
{
    class Environment
    {
        public static Option<string> GetEnvironmentVariable(string environmentVariable)
        {
            if (string.IsNullOrWhiteSpace(environmentVariable))
                return None<string>();

            try {
                var value = System.Environment.GetEnvironmentVariable(environmentVariable);
                return string.IsNullOrWhiteSpace(value) ? None<string>() : Some(value);
            } catch {
                return None<string>();
            }
        }
    }
}

#endif
