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
            playerData.text += "���\n";
        if(datas.OrderBy(d => d.gainDamage).First() == data)
            playerData.text += "���� ���� ���ظ� ���� �÷��̾�\n";
        if(datas.OrderBy(d => d.fearAmount).First() == data)
            playerData.text += "���� ���ظ����� ���� �÷��̾�\n";
    }
}
