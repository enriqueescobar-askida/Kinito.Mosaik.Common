// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializeObject.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the SerializeObject type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Alphamosaik.Common.Library
{
    public class SerializeObject<T>
    {
        public static byte[] Object2ByteArray(T o)
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, o);
                return ms.ToArray();
            }
        }

        public static T ByteArray2Object(byte[] b)
        {
            using (var ms = new MemoryStream(b))
            {
                var bf = new BinaryFormatter();
                ms.Position = 0;
                return (T)bf.Deserialize(ms);
            }
        }
    }
}
