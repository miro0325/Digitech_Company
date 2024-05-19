using System;
using UnityEngine;

public static class JsonSerializer
{
    [Serializable]
    public class ArrayWrapper<TArray>
    {
        public TArray[] array;

        public ArrayWrapper(TArray[] array)
            => this.array = array;
    }

    public static string ClassToJson<TClass>(TClass @class)
    {
        return JsonUtility.ToJson(@class);
    }

    public static string ArrayToJson<TArray>(TArray[] array)
    {
        var wrapper = new ArrayWrapper<TArray>(array);
        return JsonUtility.ToJson(wrapper);
    }

    public static TClass JsonToClass<TClass>(string json)
    {
        return JsonUtility.FromJson<TClass>(json);
    }

    public static TArray[] JsonToArray<TArray>(string json)
    {
        return JsonUtility.FromJson<ArrayWrapper<TArray>>(json).array;
    }

    public static string ToJson<TClass>(this TClass @class) where TClass : class
    {
        return ClassToJson(@class);
    }

    public static string ToJson<TArray>(this TArray[] array)
    {
        return ArrayToJson(array);
    }
}