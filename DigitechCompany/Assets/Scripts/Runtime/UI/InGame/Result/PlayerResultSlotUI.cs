using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerResultSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI playerData;

    public void Initialize(GameManager.PlayerData data)
    {
        playerName.text = data.playerName;
        playerName.fontStyle = FontStyles.Strikethrough;
        
        playerData.text = "";
        if(data.isAlive)
            playerData.text += "���\n";
        if(data.isGainMaxDamage)
            playerData.text += "���� ���� ���ظ� ���� �÷��̾�\n";
        if(data.isMostParanoia)
            playerData.text += "���� ���ظ����� ���� �÷��̾�\n";
    }
}
