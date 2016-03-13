namespace Broken_Shadows.Patterns
{
    public class Singleton<T> where T : new()
    {
        private static T instance;
        public static T Get()
        {
            if (instance == null)
            {
                instance = new T();
                return instance;
            }
            else
            {
                return instance;
            }
        }
    }
}
