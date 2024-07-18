using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class InGameFadeInUI : MonoBehaviour
{
    private void Start()
    {
        ServiceLocator
            .For(this)
            .Get<GameManager>()
            .OnLoadComplete += () =>
            {
                GetComponent<Image>()
                    .DOColor(default, 2f)
                    .OnComplete(() => gameObject.SetActive(false));
            };
    }

    public void FadeOut()
    {
        gameObject.SetActive(true);
        GetComponent<Image>()
            .DOColor(Color.black, 2f);
    }
}
