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
    /// Dictionary�� Json���� �Ľ��ϱ�
    /// </summary>
    /// <typeparam name="TKey">Dictionary Key�� ����</typeparam>
    /// <typeparam name="TValue">Dictionary Value�� ����</typeparam>
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
    /// Json Data�� �ٽ� Dictionary�� �Ľ��ϱ�
    /// </summary>
    /// <typeparam name="TKey">Dictionary Key�� ����</typeparam>
    /// <typeparam name="TValue">Dictionary Value�� ����</typeparam>
    /// <param name="jsonData">�Ľ̵Ǿ��� ������</param>
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