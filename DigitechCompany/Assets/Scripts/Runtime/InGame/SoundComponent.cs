using UnityEngine;

public class SoundComponent : MonoBehaviour
{
    public void Play(float volume, bool loop = true)
    {
        var source = GetComponent<AudioSource>();

        source.volume = volume;
        source.Play();

        if(loop) source.loop = true;
        else this.Invoke(() => Destroy(gameObject), source.clip.length);
    }
}