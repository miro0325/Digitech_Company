using UnityEngine;

[CreateAssetMenu(menuName = "Command/Moons")]
public class Moons : Command
{
    public override string Activate(string cmd, string[] args = null)
    {
        return GetExplainText(cmd, args);
    }

    protected override string GetExplainText(string cmd, string[] args)
    {
        return explain;
    }
}