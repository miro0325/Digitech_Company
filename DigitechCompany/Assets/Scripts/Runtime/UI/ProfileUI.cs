using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    private Player player;
    private InGameLoader inGameLoader;

    [SerializeField] private Image hp;
    [SerializeField] private Image stamina;

    private void Start()
    {
        inGameLoader = ServiceLocator.For(this).Get<InGameLoader>();
        inGameLoader.OnLoadComplete += () =>
            player = ServiceLocator.GetEveryWhere<Player>();
    }

    private void Update()
    {
        if (ReferenceEquals(player, null)) return;

        hp.color = new Color(1, 0, 0, 1 - (player.CurStats.GetStat(Stats.Key.Hp) / player.MaxStats.GetStat(Stats.Key.Hp)));
        stamina.fillAmount = Mathf.Lerp(0, 0.4f, player.CurStats.GetStat(Stats.Key.Stamina) / player.MaxStats.GetStat(Stats.Key.Stamina));

    }
}
