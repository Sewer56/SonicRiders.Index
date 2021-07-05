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
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        private static readonly string[] _sizeUnits = { "B", "KiB", "MiB", "GiB", "TiB" };

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
        public static string GetValidDirectory(string label)
        {
            while (true)
            {
                Console.Write(label + ": ");
                string path = TrimQuotes(Console.ReadLine());

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
        public static string GetValidFile(string label)
        {
            while (true)
            {
                Console.Write(label + ": ");
                string path = TrimQuotes(Console.ReadLine());

                if (!File.Exists(path))
                {
                    Console.WriteLine($"File {path} does not exist.");
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

        /// <summary>
        /// Converts a file size to a user friendly format.
        /// </summary>
        /// <param name="fileSize">Size of file.</param>
        public static string ToUserFriendlyFileSize(long fileSize)
        {
            var size = (float) fileSize;
            const float divisor = 1024;
            int order = 0;
            while (size >= divisor && order < _sizeUnits.Length - 1)
            {
                order++;
                size = size / divisor;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return $"{size:0.00} {_sizeUnits[order]}";
        }

        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="value">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        public static T GetAttributeOfType<T>(this Enum value) where T : System.Attribute
        {
            var type = value.GetType();
            var memInfo = type.GetMember(value.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        private static string TrimQuotes(string path)
        {
            return path.TrimStart('"').TrimEnd('"');
        }
    }
}
