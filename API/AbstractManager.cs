using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimlibs.API
{
    class AbstractManager<T> where T : new()
    {
        private static T _instance;

        /// <summary>
        /// Use this to initialize variables instead of overriding the constructor
        /// </summary>
        public virtual void Initialize() { }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                }

                return _instance;
            }
        }

        protected AbstractManager()
        {
            Initialize();
        }

        protected virtual void UnloadInternal() { }

        public void Unload()
        {
            UnloadInternal();
            _instance = default;
        }
    }
}
