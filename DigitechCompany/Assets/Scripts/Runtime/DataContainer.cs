using UnityEngine;

namespace Game
{
    public class DataContainer : MonoBehaviour
    {
        public SettingData settingData;

        private void Awake()
        {
            ServiceProvider.Register(this, true);
        }
    }
}