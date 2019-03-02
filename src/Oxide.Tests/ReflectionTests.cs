using System;
using Oxide;
using Xunit;

namespace Oxide.Tests
{
    public class ReflectionTests
    {
        [Fact]
        public void Trying_to_create_instance_of_assignable_fails_if_type_is_not_assignable_to_target()
        {
            var ex = Assert.Throws<ArgumentException>(() => new Assignable<string>(typeof(int)));
        }

        [Fact]
        public void Assignable_implicitly_casts_to_inner_type()
        {
            var targetType = typeof(int);
            Type assignableType = new Assignable<object>(typeof(int));
            Assert.Same(targetType, assignableType);
        }
    }
}
