using System;
using UnityEngine;

namespace Utilities
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public bool global = true;
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T) FindObjectOfType<T>();
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (global)
            {
                if (instance != null && instance != this.gameObject.GetComponent<T>())
                {
                    Destroy(gameObject);
                    return;
                }
                DontDestroyOnLoad(gameObject);
                instance = gameObject.GetComponent<T>();
            }
            OnStart();
        }

        protected virtual void OnStart()
        {
        }
    }
}