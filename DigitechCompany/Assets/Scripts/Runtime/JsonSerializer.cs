using System;
using System.Collections.Generic;
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

    [Serializable]
    public class ListWrapper<TArray>
    {
        public List<TArray> list;

        public ListWrapper(List<TArray> list)
            => this.list = list;
    }

    public static string ClassToJson<TClass>(TClass @class)
    {
        return JsonUtility.ToJson(@class);
    }

    public static string ListToJson<TList>(List<TList> list)
    {
        var wrapper = new ListWrapper<TList>(list);
        return JsonUtility.ToJson(wrapper);
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

    public static List<TList> JsonToList<TList>(string json)
    {
        return JsonUtility.FromJson<ListWrapper<TList>>((json)).list;
    }

    public static string ToJson<TClass>(this TClass @class) where TClass : class
    {
        return ClassToJson(@class);
    }

    public static string ToJson<TArray>(this TArray[] array)
    {
        return ArrayToJson(array);
    }

    public static string ToJson<TList>(this List<TList> array)
    {
        return ListToJson(array);
    }

    public static TClass ToClass<TClass>(this string json) where TClass : class
    {
        return JsonToClass<TClass>(json);
    }

    public static TArray[] ToArray<TArray>(this string json)
    {
        return JsonToArray<TArray>(json);
    }

    public static List<TList> ToList<TList>(this string json)
    {
        return JsonToList<TList>(json);
    }
}