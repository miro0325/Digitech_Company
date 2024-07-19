using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class GameInfoMoniter : MonoBehaviour
{
    private GameManager _gameManager;
    private GameManager gameManager => _gameManager ??= ServiceLocator.For(this).Get<GameManager>();

    [SerializeField] private TextMeshProUGUI text;

    private void Update()
    {
        StringBuilder builder = new();
        builder.Append($"{gameManager.CurEarn:#,##0}/{gameManager.TargetEarn:#,##0} $\n");
        builder.Append($"³²Àº ÀÏ¼ö: {gameManager.RemainDay}\n\n");
        builder.Append($"Çà¼º: {gameManager.Planet}\n\n");
        builder.Append($"³¯¾¾: ¸¼À½");
        text.text = builder.ToString();
    }
}
