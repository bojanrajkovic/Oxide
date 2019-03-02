using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Oxide
{
    static class Reflection
    {
        public static IEnumerable<Assignable<T>> GetTypesAssignableTo<T>(Assembly assembly)
        {
            var targetType = typeof(T).GetTypeInfo();
            return assembly.ExportedTypes.Where(t => targetType.IsAssignableFrom(t.GetTypeInfo()))
                           .Select(t => new Assignable<T>(t));
        }
    }

    /// <summary>
    /// Represents a type that is guaranteed to be assignable to a
    /// variable of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type that this type is assignable to.</typeparam>
    class Assignable<T>
    {
        readonly TypeInfo targetType = typeof(T).GetTypeInfo();

        internal Assignable(Type type)
        {
            TypeInfo = type.GetTypeInfo();

            if (!targetType.IsAssignableFrom(TypeInfo))
                throw new ArgumentException(
                    $"Given type {TypeInfo.FullName} is not assignable to generic type {targetType.FullName}.",
                    nameof(type)
                );

            Type = TypeInfo.AsType();
        }

        public static explicit operator Assignable<T>(Type type) => new Assignable<T>(type);
        public static implicit operator Type(Assignable<T> self) => self.TypeInfo.AsType();

        public Type Type { get; }
        public TypeInfo TypeInfo { get; }

        public T CreateInstance() => (T)Activator.CreateInstance(Type);
        public T CreateInstance(params object[] args) => (T)Activator.CreateInstance(Type, args);
    }
}
