using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using NaughtyAttributes;

[CreateAssetMenu(menuName ="Command/Purchase")]
public class Purchase : Command
{
    private ResourceLoader _resourceLoader;
    private ResourceLoader resourceLoader => _resourceLoader ??= ServiceLocator.ForGlobal().Get<ResourceLoader>();
    
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

    [SerializeField] private SOLoadData loadData;

    [Button]
    private void GetItems()
    {
        var items = loadData.itemDatas
            .Where(item => item.Value.isAvailable && item.Value.type == ItemType.Buy)
            .Select(item => item.Key)
            .Where(item => !commands.Select(c => c.cmd).Contains(item))
            .Select(item => new CmdData() { cmd = item})
            .ToArray();
        commands.AddRange(items);
    }

    public override string Activate(string cmd, string[] args)
    {
        if(args.Length == 0)
        {
            args = new string[1];
            args[0] = "1";
        }
        List<ItemBase> list;
        if(int.TryParse(args[0], out int count))
        {
            list = new List<ItemBase>(count);
            for (int i = 0; i < count; i++)
            {
                if(TryGetItem(cmd, out var item))
                    list.Add(item);
            }
        }
        else //if args are not currect
        {
            list = new List<ItemBase>(1);
            if(TryGetItem(cmd, out var item))
                list.Add(item);
            args[0] = "1";
        }
        // Delivary.Instance.AddDelivaryItems(list);
        return GetExplainText(cmd,args);
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

    private bool TryGetItem(string key, out ItemBase item)
    {
        return resourceLoader.itemPrefabs.TryGetValue(key, out item);
    }

}
