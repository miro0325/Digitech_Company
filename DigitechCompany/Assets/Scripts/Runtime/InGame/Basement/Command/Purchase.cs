using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

[CreateAssetMenu(menuName ="Command/Purchase")]
public class Purchase : Command
{
    
    
    // public override string[] Aliases
    // {
    //     get
    //     {
    //         //Debug.Log(itemList[0].Name);
    //         var list = itemList;
    //         if(itemDict.Count == 0)
    //         {
    //             foreach(var item in list)
    //             {
    //                 itemDict.Add(item.Name.ToLower(), item);
    //             }
    //         }
    //         return list.Select(x => x.Name.ToLower()).ToArray();
    //     }

    // }

    [SerializeField] ItemBase[] itemList; 
    private Dictionary<string, ItemBase> itemDict = new();

    public override string Activate(string cmd, string[] args)
    {
        if(args.Length == 0)
        {
            args = new string[1];
            args[0] = "1";
        }
        List<ItemBase> list;
        int count = 0;
        if(int.TryParse(args[0], out count))
        {
            list = new List<ItemBase>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(GetItem(cmd));
            }
        } else
        {
            list = new List<ItemBase>(1);
            list.Add(GetItem(cmd));
            args[0] = "1";
        }
        // Delivary.Instance.AddDelivaryItems(list);
        return GetExplainText(cmd,args);
    }

    public override void Init()
    {
    }

    protected override string GetExplainText(string cmd, string[] args)
    {
        string txt = explain;
        MatchCollection matches = regax.Matches(explain);
        Debug.Log(matches.Count);
        foreach(Match match in matches)
        {
            if(match.Value == "cmd")
                txt = txt.Replace($"<{match.Value}>", cmd);
            for(int i = 0; i < args.Length; i++)
            {
                if (match.Value == null || string.IsNullOrEmpty(args[i])) continue;
                
                if(match.Value == $"args[{i}]")
                {
                    txt = txt.Replace($"<{match.Value}>", int.Parse(args[i]).ToString());

                }
            }
        }
        return txt;
    }

    private ItemBase GetItem(string key)
    {
        if(itemDict.TryGetValue(key, out var item))
        {
            return item;
        }
        return null;
    }

}
