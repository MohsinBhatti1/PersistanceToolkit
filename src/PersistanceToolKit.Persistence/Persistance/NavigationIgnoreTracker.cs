namespace PersistanceToolKit.Persistence.Persistance
{
    internal static class NavigationIgnoreTracker
    {
        private static readonly Dictionary<Type, List<string>> _pendingIgnores = new();

        internal static void MarkIgnored<T>(string propertyName)
        {
            var type = typeof(T);
            if (!_pendingIgnores.ContainsKey(type))
                _pendingIgnores[type] = new List<string>();

            _pendingIgnores[type].Add(propertyName);
        }

        internal static Dictionary<Type, List<string>> CollectAndReset()
        {
            var copy = new Dictionary<Type, List<string>>(_pendingIgnores);
            _pendingIgnores.Clear();
            return copy;
        }
    }
}
