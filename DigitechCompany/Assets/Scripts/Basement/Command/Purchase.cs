using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

[CreateAssetMenu(menuName ="Command/Purchase")]
public class Purchase : Command
{
    
    
    public override string[] Aliases
    {
        get
        {
            return itemList;
        }

    }

    [SerializeField] string[] itemList; 

    public override string Activate(string cmd, string[] args)
    {
        if(args.Length == 0)
        {
            args = new string[1];
            args[0] = "1";
        }
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
                if(match.Value == $"args[{i}]")
                {
                    txt = txt.Replace($"<{match.Value}>", int.Parse(args[i]).ToString());

                }
            }
        }
        return txt;
    }

}
