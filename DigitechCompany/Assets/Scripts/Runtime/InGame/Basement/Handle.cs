using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handle : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioSource audioSource;

    public string GetInteractionExplain(UnitBase unit)
    {
        return "경적 울리기";
    }

    public float GetInteractRequireTime(UnitBase unit)
    {
        return 0;
    }

    public InteractID GetTargetInteractID(UnitBase unit)
    {
        return InteractID.ID1;
    }

    public bool IsInteractable(UnitBase unit)
    {
        if(audioSource.isPlaying)
            return false;
        else
            return true;
    }

    public void OnInteract(UnitBase unit)
    {
        if (audioSource.isPlaying || audioSource.clip == null) return;
        audioSource.Play();
    }
}
