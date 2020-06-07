using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PhysicsAudio : MonoBehaviour
{
    private AudioSource audioSource;
    public PlayAudio audio;

    public float playWaitTime, minVelocity, maxVelocity;

    private bool canPlay = true;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void ResetPlay()
    {
        canPlay = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!canPlay)
        {
            return;
        }

        canPlay = false;
        Invoke("ResetPlay", playWaitTime);

        float velocity = collision.relativeVelocity.magnitude;
        if (velocity > minVelocity)
        {
            audio.Play(audioSource, velocity, minVelocity, maxVelocity);
        }
    }
}
