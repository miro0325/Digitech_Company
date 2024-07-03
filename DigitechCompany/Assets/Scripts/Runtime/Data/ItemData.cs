using System;
using System.Text.RegularExpressions;
using UnityEngine;

public enum ItemType { Sell, Buy }

[Serializable]
public class ItemData
{
    public string key;
    public string name;
    public float weight;
    public float sellPriceMin;
    public float sellPriceMax;
    public float buyPrice;
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
        float.TryParse(split[count++], out data.sellPriceMin);
        float.TryParse(split[count++], out data.sellPriceMax);
        float.TryParse(split[count++], out data.buyPrice);
        bool.TryParse(split[count++], out data.isTwoHand);
        Enum.TryParse(split[count++], out data.type);
        data.explain = split[count++];
        return data;
    }
}