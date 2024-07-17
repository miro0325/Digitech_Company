using System;
using System.Collections.Generic;
using UnityEngine;

public enum Sound
{

}

public class SoundManager : MonoBehaviour, IService
{
    [Serializable]
    public class SoundData
    {
        public Sound sound;
        public SoundComponent source;
    }

    private DataContainer _dataContainer;
    private DataContainer dataContainer => _dataContainer ??= ServiceLocator.ForGlobal().Get<DataContainer>();

    [SerializeField] private SoundData[] datas;
    private Dictionary<Sound, SoundComponent> sounds = new();

    private void Awake()
    {
        ServiceLocator.ForGlobal().Register(this);

        foreach (var data in datas) sounds.Add(data.sound, data.source);
    }

    public SoundComponent PlaySound(Sound sound, Vector3 position, float volume, Transform parent = null, bool loop = false)
    {
        if(sounds.TryGetValue(sound, out var source))
        {
            var instance = Instantiate(source, position, Quaternion.identity, parent);
            instance.Play(volume * dataContainer.userData.masterSound, loop);
            return instance;
        }
        else
        {
            Debug.LogWarning($"There isn't source: {sound}");
            return null;
        }
    }
}