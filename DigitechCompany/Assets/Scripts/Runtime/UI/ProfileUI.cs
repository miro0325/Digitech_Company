using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    //service
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

    [SerializeField] private Image hp;
    [SerializeField] private Image stamina;

    private void Update()
    {
        if (ReferenceEquals(Player, null)) return;

        hp.color = new Color(1, 0, 0, 1 - (Player.CurStats.GetStat(Stats.Key.Hp) / Player.MaxStats.GetStat(Stats.Key.Hp)));
        stamina.fillAmount = Mathf.Lerp(0, 0.4f, Player.CurStats.GetStat(Stats.Key.Stamina) / Player.MaxStats.GetStat(Stats.Key.Stamina));
    }
}
