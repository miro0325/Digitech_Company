using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    //service
    private InGamePlayer _player;
    private InGamePlayer player => _player ??= ServiceLocator.For(this).Get<InGamePlayer>();

    [SerializeField] private Image hp;
    [SerializeField] private Image stamina;
    [SerializeField] private Image itemBattery;

    private void Update()
    {
        if (ReferenceEquals(player, null)) return;

        hp.color = new Color(1, 0, 0, DOVirtual.EasedValue(0, 0.75f, 1 - (player.CurStats.GetStat(Stats.Key.Hp) / player.MaxStats.GetStat(Stats.Key.Hp)), Ease.InQuad));
        stamina.fillAmount = player.CurStats.GetStat(Stats.Key.Stamina) / player.MaxStats.GetStat(Stats.Key.Stamina);
    }
}
