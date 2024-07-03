using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using NaughtyAttributes;
using Sherbert.Framework.Generic;

[CreateAssetMenu(menuName = "Scriptable/LoadData")]
public class SOLoadData : ScriptableObject
{
    public SerializableDictionary<string, ItemData> itemDatas = new();

    [Button]
    private void Load()
    {
        LoadData().Forget();
    }

    private async UniTask LoadData()
    {
        Debug.Log("Start Loading...");
        await TSVTask
        (
            "183Mza3_fsxYtgeTj6nnzgUrsUiC7jra8BIBfTp9c7AE",
            526213072,
            tsv =>
            {
                itemDatas.Clear();

                var split = tsv.Split('\n');
                for (int i = 1; i < split.Length; i++)
                {
                    var itemData = ItemData.Parse(split[i]);
                    itemDatas.Add(itemData.key, itemData);
                }
            }
        );
        Debug.Log("Item Loading Complete"); 
        Debug.Log("Loading Complete"); 
    }

    private async UniTask TSVTask(string address, long gid, Action<string> action)
    {
        var request = UnityWebRequest.Get($"https://docs.google.com/spreadsheets/d/{address}/export?format=tsv&gid={gid}");
        await request.SendWebRequest();
        action?.Invoke(request.downloadHandler.text);
    }
}