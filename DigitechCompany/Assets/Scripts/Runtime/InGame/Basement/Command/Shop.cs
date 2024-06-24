using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Shop : Command
{
    public override string Activate(string cmd, string[] args = null)
    {
        return GetExplainText(cmd, args);
    }

    public override void Init()
    {

    }

    protected override string GetExplainText(string cmd, string[] args)
    {
        return "";
    }

}
