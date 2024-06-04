using TMPro;
using UnityEngine;

public class TestStaminaDisplayer : MonoBehaviour
{
    private InGamePlayer player;
    private InGamePlayer Player
    {
        get
        {
            if(ReferenceEquals(player, null))
                player = ServiceLocator.For(this).Get<InGamePlayer>();
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