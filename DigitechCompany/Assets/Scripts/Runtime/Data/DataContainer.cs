using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Networking;

public class DataContainer : MonoBehaviour
{
    private ProjectInitializer projectInitializer => Services.Get<ProjectInitializer>();

    //load
    public Dictionary<string, ItemData> itemDatas = new();

    //user
    public UserData userData;

    private void Awake()
    {
        Services.Register(this, true);

        // projectInitializer.AddTask(new LoadingTaskData(() =>
        // {
        //     return TSVTask
        //     (
        //         "183Mza3_fsxYtgeTj6nnzgUrsUiC7jra8BIBfTp9c7AE",
        //         526213072,
        //         tsv =>
        //         {
        //             var split = tsv.Split('\n');
        //             for (int i = 1; i < split.Length; i++)
        //             {
        //                 var itemData = ItemData.Parse(split[i]);
        //                 itemDatas.Add(itemData.key, itemData);
        //             }
        //         }
        //     );
        // }));
    }

    public async UniTask Load()
    {
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
    }

    private async UniTask TSVTask(string address, long gid, Action<string> action)
    {
        var request = UnityWebRequest.Get($"https://docs.google.com/spreadsheets/d/{address}/export?format=tsv&gid={gid}");
        await request.SendWebRequest();
        action?.Invoke(request.downloadHandler.text);
    }
}