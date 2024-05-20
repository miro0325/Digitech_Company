using Photon.Pun;
using UnityEngine;

public class TestBasement : MonoBehaviourPun, IService
{
    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }
}