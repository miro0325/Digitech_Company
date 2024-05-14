using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    private Player player;

    [SerializeField] private Image hp;
    [SerializeField] private Image stamina;

    private void Start()
    {
        player = Services.Get<Player>();
    }

    private void Update()
    {
        hp.color = new Color(1, 0, 0, 1 - (player.CurStats.GetStat(Stats.Key.Hp) / player.MaxStats.GetStat(Stats.Key.Hp)));
        stamina.fillAmount = Mathf.Lerp(0, 0.4f, player.CurStats.GetStat(Stats.Key.Stamina) / player.MaxStats.GetStat(Stats.Key.Stamina));
    }
}
