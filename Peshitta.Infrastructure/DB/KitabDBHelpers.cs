using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Peshitta.Infrastructure.DB
{
    public static class KitabDBHelpers
    {
        public static T LoadUncompress<T>(this FileInfo fileInfo) where T: class
        {
            //TODO: uncompress first+
            var ret = JsonSerializer.Deserialize<T>(fileInfo.OpenRead());
            return ret;
        }

        public static T LoadUncompressText<T>(this FileInfo fileInfo, out DateTime lastWriteTimeUtc) where T: class
        {
            var textReader = new StreamReader(
                     new DeflateStream(
                         fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read),
                         CompressionMode.Decompress));
            string l;
            lastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
            var lst = new List<string>(9000);
            while ((l = textReader.ReadLine()) != null)
            {
                lst.Add(l);
            }
            return  (T)(System.Collections.IList)lst.ToArray();
        }
        public static bool IsCollection(this Type type)
        {
            return type.IsCollection(out Type elementType);
        }

        public static bool IsCollection(this Type type, out Type elementType)
        {
            elementType = type ?? throw new ArgumentNullException(nameof(type));

            // see if this type should be ignored.
            if (type == typeof(string))
            {
                return false;
            }

            Type collectionInterface
                = type.GetInterfaces()
                    .Union(new[] { type })
                    .FirstOrDefault(
                        t => t.GetTypeInfo().IsGenericType
                             && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (collectionInterface != null)
            {
                elementType = collectionInterface.GetGenericArguments()[0];
                return true;
            }

            return false;
        }
    }
}
