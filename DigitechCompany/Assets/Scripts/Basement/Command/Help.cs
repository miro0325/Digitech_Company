using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace TerminalCommand
{
    [CreateAssetMenu(menuName = "Command/Help")]
    public class Help : Command
    {
        public override string Activate(string cmd, string[] args)
        {

            return GetExplainText(cmd, args);
        }

        protected override string GetExplainText(string cmd, string[] args)
        {
            string txt = explain;
            MatchCollection matches = regax.Matches(explain);
            Debug.Log(matches.Count);
            foreach (Match match in matches)
            {
                if (match.Value == "cmd")
                    txt = txt.Replace($"<{match.Value}>", cmd);
                for (int i = 0; i < args.Length; i++)
                {
                    if (match.Value == $"args[{i}]")
                    {
                        txt = txt.Replace($"<{match.Value}>", int.Parse(args[i]).ToString());

                    }
                }
            }
            return txt;
        }
    }
}

