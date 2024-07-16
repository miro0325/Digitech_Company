using UnityEngine;

[CreateAssetMenu(menuName ="Command/PlanetMove")]
public class PlanetMove : Command
{
    private GameManager _gameManager;
    private GameManager gameManager => _gameManager ??= ServiceLocator.ForActiveScene().Get<GameManager>();

    public override string Activate(string cmd, string[] args = null)
    {
        gameManager.ChangePlanet(cmd);
        return GetExplainText(cmd, args);
    }

    public override void Init()
    {
    }

    protected override string GetExplainText(string cmd, string[] args)
    {
        return $"Âø·ú Çà¼º: {cmd}";
    }
}