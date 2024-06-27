using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class ByteConverter
{
    public static object ToObject(byte[] buffer)
    {
        try
        {
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                stream.Position = 0;
                return binaryFormatter.Deserialize(stream);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(exception.ToString());
        }
        return null;
    }

    public static byte[] ToByte(object obj)
    {
        try
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, obj);
                return stream.ToArray();
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(exception.ToString());
        }
        return null;
    }
}