using UnityEngine;
using Game.Data;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Networking;

namespace Game.Service
{
    public class DataContainer : MonoBehaviour
    {
        private ProjectInitializer projectInitializer => Services.Get<ProjectInitializer>();

        //load
        public List<SellItemData> sellItemData = new();
        public List<BuyItemData> buyItemData = new();
        //user
        public SettingData settingData;

        private void Awake()
        {
            Services.Register(this, true);

            projectInitializer.AddTask(new LoadingTaskData(() =>
            {
                return TSVTask
                (
                    "183Mza3_fsxYtgeTj6nnzgUrsUiC7jra8BIBfTp9c7AE",
                    0,
                    tsv =>
                    {
                        var split = tsv.Split('\n');
                        for (int i = 1; i < split.Length; i++)
                        {
                            var data = SellItemData.Parse(split[i]);
                            if(data != null)
                                sellItemData.Add(data);

                        }
                    }
                );
            }));
        }

        private async UniTask TSVTask(string address, long gid, Action<string> action)
        {
            var request = UnityWebRequest.Get($"https://docs.google.com/spreadsheets/d/{address}/export?format=tsv&gid={gid}");
            await request.SendWebRequest();
            action?.Invoke(request.downloadHandler.text);
        }
    }
}