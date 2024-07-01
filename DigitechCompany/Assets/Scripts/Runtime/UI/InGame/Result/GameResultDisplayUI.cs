using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GameResultDisplayUI : MonoBehaviour
{
    private GameManager _gameManager;
    private GameManager gameManager => _gameManager ??= ServiceLocator.For(this).Get<GameManager>();
    private Basement _basement;
    private Basement basement => _basement ??= ServiceLocator.For(this).Get<Basement>();

    [SerializeField] private RectTransform itemResultListParent;
    [SerializeField] private ItemResultSlotUI itemResultSlotUIPrefab;
    [SerializeField] private RectTransform playerResultListParent;

    private List<ItemResultSlotUI> list = new();

    private void Start()
    {
        gameManager
            .ObserveEveryValueChanged(g => g.State)
            .Where(state => state == GameState.DisplayResult)
            .Subscribe(_ => DisplayRoutine().Forget());
    }

    private async UniTask DisplayRoutine()
    {
        list.ForEach(x => Destroy(x.gameObject));
        list.Clear();

        foreach (var item in basement.CurGameItems)
        {
            var slot = Instantiate(itemResultSlotUIPrefab, itemResultListParent);
            slot.Initialize(item.Value);
            list.Add(slot);

            await UniTask.WaitForSeconds(0.1f);
        }
    }
}
