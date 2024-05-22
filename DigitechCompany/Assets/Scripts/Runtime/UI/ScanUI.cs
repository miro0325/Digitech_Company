using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class ScanUI : MonoBehaviour
{
    //const
    private const int UIPoolMaxCount = 20;

    //service
    private Player player;
    private Player Player
    {
        get
        {
            if (ReferenceEquals(player, null))
                player = ServiceLocator.For(this).Get<Player>();
            return player;
        }
    }

    //inspector
    [SerializeField] private ScanInfomationUI scanInfomationUIPrefab;

    //field
    private bool isInitialized;
    private Queue<ScanInfomationUI> uiPool = new();

    private void Update()
    {
        if (ReferenceEquals(Player, null)) return;
        if (isInitialized) return;

        isInitialized = true;

        for (int i = 0; i < UIPoolMaxCount; i++)
        {
            var inst = Instantiate(scanInfomationUIPrefab, transform);
            inst.gameObject.SetActive(false);
            inst.Initialize(PoolUIToQueue);
            uiPool.Enqueue(inst);
        }

        Player
            .ObserveEveryValueChanged(p => p.ScanData)
            .Skip(1)
            .Subscribe(scandata =>
            {
                foreach (var item in scandata.items)
                    GetUIFromQueue().StartDisplay(item);
            });
    }

    private ScanInfomationUI GetUIFromQueue()
    {
        if (uiPool.TryDequeue(out var inst))
        {
            inst.Initialize(PoolUIToQueue);
            return inst;
        }

        return Instantiate(scanInfomationUIPrefab, transform);
    }

    private void PoolUIToQueue(ScanInfomationUI ui)
    {
        if (uiPool.Count >= UIPoolMaxCount)
            Destroy(ui.gameObject);
        else
            uiPool.Enqueue(ui);
    }
}