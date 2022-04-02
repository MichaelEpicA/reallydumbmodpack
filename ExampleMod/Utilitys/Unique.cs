using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleMod
{
    static class Unique
    {
        public static void AddUnique<T>(this Il2CppSystem.Collections.Generic.List<T> self, T item)
            where T : IDisconnectHandler
        {
            if (!self.Contains(item)) self.Add(item);
        }
        public static void ForEach<T>(this IList<T> self, Action<T> todo)
        {
            for (int i = 0; i < self.Count; i++)
            {
                todo(self[i]);
            }
        }
    }
}
