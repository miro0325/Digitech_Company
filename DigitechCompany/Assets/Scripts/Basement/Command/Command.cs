using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


public abstract class Command : ScriptableObject
{
    protected Regex regax = new Regex(@"(?<=\<)(.*?)(?=\>)");
    public virtual string Cmd => command;
    public virtual string[] Aliases => aliases;
    ///<summary>
    ///명령어를 여러 개 사용하는가 
    ///</summary>
    public bool IsMultiple => isMultipleCommand;

    [SerializeField] protected string command;
    [SerializeField] protected string[] aliases;
    [SerializeField] protected bool isMultipleCommand;
    [SerializeField,TextArea(10,50)] protected string explain;

    public abstract string Activate(string cmd, string[] args = null);

    protected abstract string GetExplainText(string cmd, string[] args);
}
