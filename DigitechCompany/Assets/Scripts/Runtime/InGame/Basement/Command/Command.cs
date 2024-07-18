using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public abstract class Command : ScriptableObject
{
    [System.Serializable]
    public class CmdData
    {
        public string cmd;
        public string[] aliases;
    }
    
    protected Regex regax = new Regex(@"(?<=\<)(.*?)(?=\>)");

    [SerializeField] protected List<CmdData> commandDatas;
    [SerializeField,TextArea(10,50)] protected string explain;

    public List<CmdData> CommandDatas => commandDatas;

    public abstract string Activate(string cmd, string[] args = null);
    protected abstract string GetExplainText(string cmd, string[] args);
}
