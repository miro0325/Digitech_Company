using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    private InGamePlayer _player;
    private InGamePlayer player => _player ??= ServiceLocator.For(this).Get<InGamePlayer>();

    [SerializeField] private GameObject play;
    [SerializeField] private GameObject spectate;

    private void Update()
    {
        play.SetActive(player.gameObject.activeSelf);
        spectate.SetActive(!player.gameObject.activeSelf);
    }
}
