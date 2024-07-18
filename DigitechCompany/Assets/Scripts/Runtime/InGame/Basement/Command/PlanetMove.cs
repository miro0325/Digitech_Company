using UnityEngine;

[CreateAssetMenu(menuName ="Command/PlanetMove")]
public class PlanetMove : Command
{
    private GameManager gameManager => ServiceLocator.ForSceneOf("InGame").Get<GameManager>();

    public override string Activate(string cmd, string[] args = null)
    {
        Debug.Log(gameManager);
        gameManager.ChangePlanet(cmd);
        return GetExplainText(cmd, args);
    }

    protected override string GetExplainText(string cmd, string[] args)
    {
        return $"Âø·ú Çà¼º: {cmd}";
    }
}