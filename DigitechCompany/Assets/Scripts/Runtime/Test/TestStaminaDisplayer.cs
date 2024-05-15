using TMPro;
using UnityEngine;

public class TestStaminaDisplayer : MonoBehaviour
{
    private Player player;

    private TextMeshProUGUI text;

    private void Start()
    {
        player = ServiceLocator.GetEveryWhere<Player>();
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        text.text = player.CurStats.GetStat(Stats.Key.Stamina).ToString("0.00");
    }
}