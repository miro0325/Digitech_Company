using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
}
