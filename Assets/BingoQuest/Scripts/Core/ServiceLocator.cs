using System;
using System.Collections.Generic;

namespace BingoQuest.Core
{
    /// <summary>
    /// Lightweight service locator used for global singletons (EventBus, etc.).
    /// Prefer constructor injection where possible; use this for cross-cutting services
    /// (e.g. bootstrapped once in GameBootstrap and accessed by gameplay systems).
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        /// <summary>Registers a service instance. Overwrites any existing registration.</summary>
        public static void Register<T>(T service) where T : class
        {
            if (service is null) throw new ArgumentNullException(nameof(service));
            _services[typeof(T)] = service;
        }

        /// <summary>
        /// Retrieves the registered service of type <typeparamref name="T"/>.
        /// Throws <see cref="InvalidOperationException"/> if not registered.
        /// </summary>
        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;
            throw new InvalidOperationException(
                $"Service of type '{typeof(T).FullName}' is not registered. " +
                "Ensure it is registered in GameBootstrap before use.");
        }

        /// <summary>Returns the service or null if not registered.</summary>
        public static T? TryGet<T>() where T : class =>
            _services.TryGetValue(typeof(T), out var service) ? (T)service : null;

        /// <summary>Returns true when the service is registered.</summary>
        public static bool IsRegistered<T>() where T : class =>
            _services.ContainsKey(typeof(T));

        /// <summary>Removes all registered services (used in tests).</summary>
        public static void Reset() => _services.Clear();
    }
}
