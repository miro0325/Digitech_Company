using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName ="Command/Shop")]
public class Shop : Command
{
    [SerializeField] private SOLoadData loadData;

    public override string Activate(string cmd, string[] args = null)
    {
        return GetExplainText(cmd, args);
    }

    protected override string GetExplainText(string cmd, string[] args)
    {
        var itemDatas = loadData.itemDatas
            .Where(item => item.Value.isAvailable && item.Value.type == ItemType.Buy)
            .ToArray();

        StringBuilder sb = new();
        foreach(var item in itemDatas)
        {
            sb.Append(item.Key);
            sb.Append("\t");
            sb.Append(item.Value.buyPrice);
            sb.Append("$");
            sb.Append("\n");
        }
        
        return sb.ToString();
    }
}