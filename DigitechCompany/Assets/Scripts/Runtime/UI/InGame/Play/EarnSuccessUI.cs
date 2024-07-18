using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;

public class EarnSuccessUI : MonoBehaviour
{
    private GameManager _gameManager;
    private GameManager gameManager => _gameManager ??= ServiceLocator.For(this).Get<GameManager>();

    [SerializeField] private GameObject successUI;
    [SerializeField] private GameObject nextTargetEarn;
    [SerializeField] private TextMeshProUGUI nextTargetEarnText;

    private void Start()
    {
        gameManager
            .ObserveEveryValueChanged(gm => gm.RemainDay)
            .Where(day => day == 0 && gameManager.CurEarn >= gameManager.TargetEarn)
            .Subscribe(_ => SuccessTask().Forget());
    }

    private async UniTask SuccessTask()
    {
        successUI.SetActive(true);
        await UniTask.WaitForSeconds(4f);
        successUI.SetActive(false);

        nextTargetEarn.SetActive(true);
        for (float i = 0; i < 3; i += Time.deltaTime)
        {
            nextTargetEarnText.text = $"{Mathf.Lerp(0, gameManager.TargetEarn, i / 3):#,###.#}$";
            await UniTask.NextFrame();
        }
        await UniTask.WaitForSeconds(1f);
        nextTargetEarn.SetActive(false);
    }
}
