using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Broken_Shadows.Patterns
{
    public class Singleton<T> where T : new()
    {
        private static T _instance;
        public static T Get()
        {
            if (_instance == null)
            {
                _instance = new T();
                return _instance;
            }
            else
            {
                return _instance;
            }
        }
    }
}
