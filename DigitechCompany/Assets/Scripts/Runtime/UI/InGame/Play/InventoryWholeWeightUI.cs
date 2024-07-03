using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class InventoryWholeWeightUI : MonoBehaviour
{
    private InGamePlayer _player;
    private InGamePlayer player => _player ??= ServiceLocator.For(this).Get<InGamePlayer>();

    private TextMeshProUGUI text;
    
    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();

        player
            .ObserveEveryValueChanged(p => p.Inventory.WholeWeight)
            .Subscribe(x => text.text = $"{x}/{player.MaxStats.GetStat(Stats.Key.Weight)} kg");
        
        player.MaxStats.OnStatChanged += (key, _, @new) =>
        {
            if(key == Stats.Key.Weight)
                text.text = $"{player.Inventory.WholeWeight}/{@new} kg";
        };

        text.text = $"0/{player.MaxStats.GetStat(Stats.Key.Weight)} kg";
    }
}
