using UnityEngine;

namespace Shade.Generics
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T m_Instance = null;

        /// <summary>
        /// Retrieves the instance of T without lazily instantiating any object or trying to find it in the scene.
        /// Returns true if the instance in assigned, false otherwise, and passes the instance as an output.
        /// </summary>
        public static bool TryGetInstance(out T instance)
        {
            instance = m_Instance;
            return HasInstance;
        }

        /// <summary>
        /// Lazily retrieves the instance of T. If <see cref="m_Instance"/> is null, finds for a reference in the scene and assign it.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindAnyObjectByType<T>();
                }

                return m_Instance;
            }
        }

        /// <summary>
        /// Checks whether the <see cref="m_Instance"/> variable has correctly been assigned.
        /// </summary>
        public static bool HasInstance => m_Instance != null;

        /// <summary>
        /// If true, this object will not be destroyed when loading new scenes.
        /// </summary>
        protected abstract bool ShouldPersist { get; }

        /// <summary>
        /// If true, a new instance in a scene will replace the existing one.
        /// If false, the new instance is destroyed.
        /// </summary>
        /// <remarks>
        /// Set to true for singletons that require per-scene configurations (through the inspector for instance).
        /// Set to false for any persistent game state that accumulates over playtime and shouldn't be reset by
        /// a singleton in the new scene.
        /// </remarks>
        protected abstract bool ReplaceExisting { get; }

        protected virtual void Awake()
        {
            // Shade: if no instance was assigned yet, assign the current holder of this script as the instance
            if (!HasInstance)
            {
                m_Instance = this as T;

                // Shade: If this instance should persist, mark this object as DDOL
                if (ShouldPersist)
                {
                    DontDestroyOnLoad(gameObject);
                }

                return;
            }

            if (m_Instance == this) return;

            // Shade: If there is already an instance for T (from scene transitions) and that ReplaceExisting is true,
            // destroy the old instance to replace it with the new one in the scene
            if (ReplaceExisting)
            {
                Destroy(m_Instance.gameObject);
                m_Instance = this as T;

                if (ShouldPersist)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            // Shade: Otherwise destroy the new instance instead
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
