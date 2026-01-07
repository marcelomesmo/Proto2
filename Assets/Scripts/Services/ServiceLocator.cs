using System;
using System.Collections.Generic;

namespace Services
{
    //
    //  Boilerplate Class used by Bootstrapper
    //
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Initialize()
        {
            _services.Clear();
        }

        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);

            if (_services.ContainsKey(type))
                throw new Exception($"Service already registered: {type}");

            _services[type] = service;
        }

        public static T Get<T>() where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
                return service as T;

            throw new Exception($"Service not registered: {type}");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var obj))
            {
                service = obj as T;
                return true;
            }

            service = null;
            return false;
        }
    }
}
