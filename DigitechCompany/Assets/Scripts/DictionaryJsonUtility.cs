using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DataDictionary<TKey, TValue>
{
    public TKey Key;
    public TValue Value;
}

[Serializable]
public class JsonDataArray<TKey, TValue>
{
    public List<DataDictionary<TKey, TValue>> data = new();
}

public static class DictionaryJsonUtility
{

    /// <summary>
    /// Dictionary를 Json으로 파싱하기
    /// </summary>
    /// <typeparam name="TKey">Dictionary Key값 형식</typeparam>
    /// <typeparam name="TValue">Dictionary Value값 형식</typeparam>
    /// <param name="jsonDicData"></param>
    /// <returns></returns>
    public static string ToJson<TKey, TValue>(Dictionary<TKey, TValue> jsonDicData, bool pretty = false)
    {
        JsonDataArray<TKey, TValue> arrayJson = new JsonDataArray<TKey, TValue>();
        foreach (TKey key in jsonDicData.Keys)
        {
            var dictionaryData = new DataDictionary<TKey, TValue> { Key = key, Value = jsonDicData[key] };
            arrayJson.data.Add(dictionaryData);
        }

        return JsonUtility.ToJson(arrayJson, pretty);
    }

    /// <summary>
    /// Json Data를 다시 Dictionary로 파싱하기
    /// </summary>
    /// <typeparam name="TKey">Dictionary Key값 형식</typeparam>
    /// <typeparam name="TValue">Dictionary Value값 형식</typeparam>
    /// <param name="jsonData">파싱되었던 데이터</param>
    /// <returns></returns>

    public static Dictionary<TKey, TValue> FromJson<TKey, TValue>(string jsonData)
    {
        var dataList = JsonUtility.FromJson<JsonDataArray<TKey, TValue>>(jsonData);

        Dictionary<TKey, TValue> returnDictionary = new Dictionary<TKey, TValue>();
        for (int i = 0; i < dataList.data.Count; i++)
        {
            DataDictionary<TKey, TValue> dictionaryData = dataList.data[i];
            returnDictionary.Add(dictionaryData.Key, dictionaryData.Value);
        }

        return returnDictionary;
    }
}