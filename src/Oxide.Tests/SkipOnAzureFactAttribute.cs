using Xunit;

namespace Oxide.Tests
{
    sealed class SkipOnAzureFactAttribute : FactAttribute
    {
        public SkipOnAzureFactAttribute()
        {
            if (IsAzureCI()) {
                Skip = "This test doesn't pass on Azure CI, but does pass locally.";
            }
        }

        bool IsAzureCI() => Environment.GetEnvironmentVariable("TF_BUILD") == "True";
    }
}
