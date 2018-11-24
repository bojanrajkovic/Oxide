using System;

using Xunit;

namespace Oxide.Tests
{
    public class ObjectExtensionTests
    {
        [Fact]
        public void Let_calls_action()
        {
            var self = new object();
            var called = false;
            self.Let(it => {
                called = true;
                return 5;
            });

            Assert.True(called);
        }

        [Fact]
        public void Let_returns_action_return()
        {
            var self = new object();
            var ret = self.Let(it => {
                return it.GetHashCode();
            });
            Assert.Equal(self.GetHashCode(), ret);
        }

        [Fact]
        public void Also_calls_action()
        {
            var self = new object();
            var called = false;
            var ret = self.Also(it => {
                called = true;
            });
            Assert.True(called);
        }

        [Fact]
        public void Also_returns_self()
        {
            var self = new object();
            var ret = self.Also(it => {
                it.GetHashCode();
            });
            Assert.Same(self, ret);
        }

        [Fact]
        public void Use_calls_method()
        {
            var self = new FakeDisposable();
            var called = false;
            var ret = self.Use(it => {
                called = true;
                return it;
            });
            
            Assert.True(called);
        }

        [Fact]
        public void Use_returns_block_result()
        {
            var self = new FakeDisposable();
            var ret = self.Use(it => {
                return it.GetHashCode();
            });

            Assert.Equal(self.GetHashCode(), ret);
        }

        [Fact]
        public void Use_disposes_self()
        {
            var self = new FakeDisposable();
            var ret = self.Use(it => {
                return 5;
            });

            Assert.True(self.disposed);
        }

        [Fact]
        public void To_pairs_up_objects_correctly()
        {
            var self = new object();
            var other = new object();

            var (a, b) = self.To(other);

            Assert.Same(self, a);
            Assert.Same(other, b);            
        }

        [Fact]
        public void Take_unless_returns_none_when_predicate_matches()
        {
            var self = 10;
            var res = self.TakeUnless(x => x%2 == 0);

            Assert.True(res.IsNone);
            Assert.IsType<None<int>>(res);
        }

        [Fact]
        public void Take_unless_returns_some_when_predicate_fails()
        {
            var self = 10;
            var res = self.TakeUnless(x => x%3 == 0);

            Assert.True(res.IsSome);
            Assert.IsType<Some<int>>(res);
            Assert.Equal(self, res.Unwrap());
        }

        [Fact]
        public void Take_if_returns_none_when_predicate_fails()
        {
            var self = 10;
            var res = self.TakeIf(x => x%3 == 0);

            Assert.True(res.IsNone);
            Assert.IsType<None<int>>(res);
        }

        [Fact]
        public void Take_if_returns_some_when_predicate_matches()
        {
            var self = 10;
            var res = self.TakeIf(x => x%2 == 0);

            Assert.True(res.IsSome);
            Assert.IsType<Some<int>>(res);
            Assert.Equal(self, res.Unwrap());
        }
    }

    class FakeDisposable : IDisposable
    {
        public bool disposed = false;

        public void Dispose()
        {
            disposed = true;
        }
    }
}