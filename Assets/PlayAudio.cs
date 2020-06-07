using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayAudio
{
    public AudioClip[] audioClips;

    [Range(0, 10)]
    public float minVolume;
    [Range(0, 10)]
    public float maxVolume;

    public bool scaleVolume;

    [Range(0, 10)]
    public float minPitch;
    [Range(0, 10)]
    public float maxPitch;

    public bool scalePitch;

    public void Play(AudioSource source, float scale, float minValue, float maxValue)
    {
        float volume;
        if (scaleVolume)
        {
            volume = Util.ConvertRange(scale, minValue, maxValue, minVolume, maxVolume);
        }
        else
        {
            volume = Random.Range(minVolume, maxVolume);
        }

        float pitch;
        if (scalePitch)
        {
            pitch = Util.ConvertRange(scale, minValue, maxValue, minPitch, maxPitch);
        }
        else
        {
            pitch = Random.Range(minPitch, maxPitch);
        }

        Play(source, volume, pitch);

    }

    public void Play(AudioSource source)
    {
        float volume = Random.Range(minVolume, maxVolume);
        float pitch = Random.Range(minPitch, maxPitch);
        Play(source, volume, pitch);
    }

    public void Play(AudioSource source, float volume, float pitch)
    {
        source.volume = volume;
        source.pitch = pitch;
        AudioClip swingClip = audioClips[Random.Range(0, audioClips.Length - 1)];
        source.PlayOneShot(swingClip);
    }
}
