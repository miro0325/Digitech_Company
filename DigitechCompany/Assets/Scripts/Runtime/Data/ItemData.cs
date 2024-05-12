using System;
using System.Text.RegularExpressions;
using UnityEngine;

public enum ItemType { Sell, Buy }

public class ItemData
{
    public string key;
    public string name;
    public float weight;
    public float priceMin;
    public float priceMax;
    public bool isTwoHand;
    public ItemType type;
    public string explain;

    public static ItemData Parse(string tsvRow)
    {
        var data = new ItemData();
        var split = tsvRow.Split('\t');
        var count = 1;
        data.key = split[count++];
        data.name = split[count++];
        float.TryParse(split[count++], out data.weight);
        float.TryParse(split[count++], out data.priceMin);
        float.TryParse(split[count++], out data.priceMax);
        bool.TryParse(split[count++], out data.isTwoHand);
        Enum.TryParse(split[count++], out data.type);
        data.explain = split[count++];
        return data;
    }
}