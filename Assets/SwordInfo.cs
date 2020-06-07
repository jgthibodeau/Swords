using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordInfo : MonoBehaviour
{
    public PlayAudio sliceAudio;
    public PlayAudio swingAudio;

    public Transform bladeBase;
    public Transform bladeTip;

    public bool canSlice = true;

    public Gradient trailColor;

    public float moveSpeed = 100;
    public float rotateSpeed = 1;

    public bool isUsable;

    public AudioClip drawSound;

    public float length = 3;
    public float radius = 0.1f;
    public float swingSpeed = 1f;
}
