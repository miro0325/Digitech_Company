using TMPro;
using UnityEngine;

public class TestStaminaDisplayer : MonoBehaviour
{
    private Player player;
    private Player Player
    {
        get
        {
            if(ReferenceEquals(player, null))
                player = ServiceLocator.For(this).Get<Player>();
            return player;
        }
    }

    private TextMeshProUGUI text;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        text.text = Player.CurStats.GetStat(Stats.Key.Stamina).ToString("0.00");
    }
}