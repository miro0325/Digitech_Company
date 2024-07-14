using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LobbyFadeOutUI : MonoBehaviour, IService
{
    [SerializeField] private Image image;
    
    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }

    public void FadeOut(float fadeTime, Action action = null)
    {
        FadeOutTask(fadeTime, action).Forget();
    }

    private async UniTask FadeOutTask(float fadeTime, Action action)
    {   
        image.gameObject.SetActive(true);
        image.DOColor(Color.black, fadeTime);
        await UniTask.WaitForSeconds(fadeTime);
        action?.Invoke();
    }
}