using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Image loadingImage;
    
    private async void Start()
    {
        loadingImage.color = new Color(1, 1, 1, 0);

        await UniTask.WaitForSeconds(1f);
        loadingImage.DOColor(Color.white, 0.5f);
        await UniTask.WaitForSeconds(1.5f);
        loadingImage.DOColor(new Color(1, 1, 1, 0), 0.5f);
        await UniTask.WaitForSeconds(2f);
        SceneManager.LoadScene("Lobby");
    }
}
