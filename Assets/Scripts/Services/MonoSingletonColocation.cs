using UnityEngine;

namespace Colocation.Core
{
    /// <summary>
    /// Local-only MonoBehaviour singleton. Survives scene transitions.
    /// Not a NetworkBehaviour — each client has its own instance.
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _isShuttingDown;

        public static T Instance
        {
            get
            {
                if (_isShuttingDown) return null;
                lock (_lock) { return _instance; }
            }
        }

        public static bool IsReady => _instance != null && _instance._initialized;

        private bool _initialized;

        protected virtual void Awake()
        {
            lock (_lock)
            {
                if (_instance != null && _instance != this)
                {
                    Destroy(gameObject);
                    return;
                }
                _instance = (T)this;
                DontDestroyOnLoad(gameObject);
            }
        }

        protected void MarkInitialized() => _initialized = true;

        protected virtual void OnDestroy()
        {
            lock (_lock) { if (_instance == this) _instance = null; }
        }

        protected virtual void OnApplicationQuit() => _isShuttingDown = true;
    }
}