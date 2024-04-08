using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Game.InGame
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        private void Awake()
        {
            PhotonNetwork.Instantiate("Prefabs/Player", Vector3.up * 3, Quaternion.identity);
        }
    }
}
