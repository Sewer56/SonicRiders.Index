using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using IndexTool.Structs.Attributes;

namespace IndexTool.Misc
{
    public static class Utilities
    {
        public static JsonSerializerOptions JsonSerializerOptions { get; private set; } = new JsonSerializerOptions()
        {
            WriteIndented = true,
            IncludeFields = true,
        };

        /// <summary>
        /// Converts a given object to JSON.
        /// </summary>
        public static string ToJson<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, JsonSerializerOptions);
        }

        /// <summary>
        /// Deserializes a file from JSON or returns an empty (non-null) result.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        public static T FromJsonOrEmpty<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath))
                return new T();

            try
            {
                return JsonSerializer.Deserialize<T>(File.ReadAllText(filePath), JsonSerializerOptions);
            }
            catch (Exception e)
            {
                return new T();
            }
        }

        /// <summary>
        /// Gets a valid directory from a user.
        /// </summary>
        /// <param name="label">Label to assign to the directory.</param>
        public static string PickOption(string label)
        {
            while (true)
            {
                Console.Write(label + ": ");
                string path = Console.ReadLine();

                if (!Directory.Exists(path))
                {
                    Console.WriteLine($"Directory {path} does not exist.");
                    continue;
                }

                return path;
            }
        }

        /// <summary>
        /// Gets a valid directory from a user.
        /// </summary>
        /// <param name="label">Label to assign to the directory.</param>
        public static string GetValidDirectory(string label)
        {
            while (true)
            {
                Console.Write(label + ": ");
                string path = Console.ReadLine();
                
                if (!Directory.Exists(path))
                {
                    Console.WriteLine($"Directory {path} does not exist.");
                    continue;
                }

                return path;
            }
        }

        /// <summary>
        /// Gets all types which can be instantiated and assigned to a specified type (T).
        /// </summary>
        public static Type[] GetAllInstantiableTypes<T>(Assembly asm = null)
        {
            if (asm == null)
                asm = Assembly.GetExecutingAssembly();

            var types = asm.GetTypes().Where(x => x.IsAssignableTo(typeof(T)) && !x.IsAbstract && !x.IsInterface && x.GetCustomAttribute<ReflectionIgnoreAttribute>() == null);
            return types.ToArray();
        }

        /// <summary>
        /// Creates all instances of a specified type T using reflection.
        /// </summary>
        public static T[] MakeAllInstances<T>(Assembly asm = null)
        {
            return GetAllInstantiableTypes<T>(asm).Select(x => (T)Activator.CreateInstance(x)).ToArray();
        }

        /// <summary>
        /// Assigns a string if it's not null or empty.
        /// </summary>
        public static void AssignIfNotNullOrEmpty(ref string value, string newValue)
        {
            if (!string.IsNullOrEmpty(newValue))
                value = newValue;
        }
    }
}
