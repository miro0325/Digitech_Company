using UnityEngine;

public class InGamePlayerInput : MonoBehaviour, IService
{
    
    
    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }
}