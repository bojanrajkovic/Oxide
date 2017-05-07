using System;

using Xunit;

using SEnvironment = System.Environment;

namespace Oxide.Tests
{
    public class EnvironmentTests
    {
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [Theory]
        public void Passing_null_or_whitespace_returns_none(string environmentVariable)
            => Assert.True(Environment.GetEnvironmentVariable(environmentVariable).IsNone);

        [Fact]
        public void Set_environment_variable_returns_value()
        {
            const string environmentVariable = "OXIDE_TEST_ENVIRONMENT_VARIABLE";
            var randomValue = DateTime.UtcNow.ToString("o");

            SEnvironment.SetEnvironmentVariable(environmentVariable, randomValue);

            var fetchedEnv = Environment.GetEnvironmentVariable(environmentVariable);

            Assert.True(fetchedEnv.IsSome);
            Assert.Equal(randomValue, fetchedEnv.Unwrap());
        }

        [Fact]
        public void Unset_environment_variable_returns_none()
            => Assert.True(Environment.GetEnvironmentVariable("OXIDE_TEST_UNSET_VARIABLE").IsNone);
    }
}
