using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerResultSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI playerData;

    public void Initialize(GameManager.PlayerData data, GameManager.PlayerData[] datas)
    {
        playerName.text = data.playerName;
        playerName.fontStyle = FontStyles.Strikethrough;
        
        playerData.text = "";
        if(data.isAlive)
            playerData.text += "사망\n";
        if(datas.OrderBy(d => d.gainDamage).First() == data)
            playerData.text += "가장 많은 피해를 받은 플레이어\n";
        if(datas.OrderBy(d => d.fearAmount).First() == data)
            playerData.text += "가장 피해망상이 심한 플레이어\n";
    }
}
