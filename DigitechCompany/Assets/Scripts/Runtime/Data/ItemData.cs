using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Game.Data
{
    public enum ItemType
    {
        Basic,Weapon,Decoration,Interaction
    }
    
    public class SellItemData 
    {
        public int id;
        public string name;
        public float weight;
        public bool isInteractable;
        public bool isBothHand;
        public ItemType type;

        public int[] prices = new int[2];

        public static SellItemData Parse(string tsvRow)
        {
            Debug.Log(tsvRow);
            //replaceContents = replaceContents.Replace()
            var datas = tsvRow.Trim().Split('\t');

            SellItemData data = new SellItemData();
            if (datas[1].Trim() == "" || string.IsNullOrEmpty(datas[1].Trim())) return null;
            data.id = int.Parse(datas[0]);
            data.name = datas[1].Trim();
            data.weight = float.Parse(datas[2]);
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
            switch(datas[6])
            {
                case "무기":
                    data.type = ItemType.Weapon;
                    break;
                case "아이템":
                    data.type = ItemType.Basic;
                    break;
                case "상호작용":
                    data.type= ItemType.Interaction;
                    break;
                default:
                    Debug.LogError("Can't Parse Data Contents");
                    break;
            }
            if(datas[7] == "양손")
            {
                data.isBothHand = true;
            } 
            else if(datas[7] == "한손")
            {
                data.isBothHand = false;
            } 
            else
            {
                Debug.LogError("Can't Parse Data Contents");
            }
            return data;
        }
    }

    public class BuyItemData
    {
        public int id;
        public string name;
        public float weight;
        public bool isInteractable;
        public bool isBothHand;
        public ItemType type;

        public int[] prices = new int[2];

        public static SellItemData Parse(string tsvRow)
        {
            Debug.Log(tsvRow);
            //replaceContents = replaceContents.Replace()
            var datas = tsvRow.Trim().Split('\t');

            SellItemData data = new SellItemData();
            data.id = int.Parse(datas[0]);
            data.name = datas[1].Trim();
            data.weight = float.Parse(datas[2]);
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
                case "무기":
                    data.type = ItemType.Weapon;
                    break;
                case "아이템":
                    data.type = ItemType.Basic;
                    break;
                case "상호작용":
                    data.type = ItemType.Interaction;
                    break;
                default:
                    Debug.LogError("Can't Parse Data Contents");
                    break;
            }
            if (datas[7] == "양손")
            {
                data.isBothHand = true;
            }
            else if (datas[7] == "한손")
            {
                data.isBothHand = false;
            }
            else
            {
                Debug.LogError("Can't Parse Data Contents");
            }
            return data;
        }
    }
}