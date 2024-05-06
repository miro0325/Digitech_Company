using System;
using System.Text.RegularExpressions;
using UnityEngine;

public enum ItemType
{
    Basic, Weapon, Decoration, Interaction
}

public class ItemData
{
    public int id;
    public string name;
    public float weight;
    public bool isInteractable;
    public bool isBothHand;
    public bool isOnlySell;
    public ItemType type;

    public int[] prices = new int[2];

    public static ItemData Parse(string tsvRow)
    {
        Debug.Log(tsvRow);
        var datas = tsvRow.Trim().Split('\t');

        ItemData data = new ItemData();
        if (!bool.Parse(datas[10].ToLower())) return null;
        data.id = int.Parse(datas[0]);
        data.name = datas[1].Trim();
        data.weight = float.Parse(datas[2]);
        Debug.Log(datas[2] + " " + datas[3]);
        data.prices[0] = int.Parse(datas[3]);
        data.prices[1] = int.Parse(datas[4]);
        if (datas[5] == "O")
        {
            data.isInteractable = true;
        }
        else if (datas[5] == "X")
        {
            data.isInteractable = false;
        }
        else
        {
            Debug.LogError("Can't Parse Data Contents");
        }
        switch (datas[6])
        {
            case "����":
                data.type = ItemType.Weapon;
                break;
            case "������":
                data.type = ItemType.Basic;
                break;
            case "��ȣ�ۿ�":
                data.type = ItemType.Interaction;
                break;
            default:
                Debug.LogError("Can't Parse Data Contents");
                break;
        }
        if (datas[7] == "���")
        {
            data.isBothHand = true;
        }
        else if (datas[7] == "�Ѽ�")
        {
            data.isBothHand = false;
        }
        else
        {
            Debug.LogError("Can't Parse Data Contents");
        }
        if (datas[8] == "�Ǹ�")
        {
            data.isOnlySell = true;
        }
        else
        {
            data.isOnlySell = false;
        }
        return data;
    }
}