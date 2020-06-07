using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordToggle : MonoBehaviour
{
    public SwordInfo[] swords;
    public int currentSword = 0;

    public UnityStandardAssets.Utility.FollowTarget baseFollow, tipFollow;

    private MeleeWeaponTrail sliceTrail;
    private AudioSource audioSource;

    void Start()
    {
        sliceTrail = GetComponent<MeleeWeaponTrail>();
        audioSource = GetComponent<AudioSource>();

        for (int i = 0; i < swords.Length; i++)
        {
            DeactivateSword(i);
        }

        ActivateSword(currentSword);
    }

    void Update()
    {
        int nextSword = currentSword;
        if (Input.GetButtonDown("Next"))
        {
            do
            {
                nextSword++;
                if (nextSword > swords.Length - 1)
                {
                    nextSword = 0;
                }
            } while (!swords[nextSword].isUsable && nextSword != currentSword);
        }
        if (Input.GetButtonDown("Previous"))
        {
            do
            {
                nextSword--;
                if (nextSword < 0)
                {
                    nextSword = swords.Length - 1;
                }
            } while (!swords[nextSword].isUsable && nextSword != currentSword);
        }

        if (nextSword != currentSword)
        {
            ChangeSword(nextSword);
        }
    }

    void ChangeSword(int nextSword)
    {
        DeactivateSword(currentSword);
        currentSword = nextSword;
        ActivateSword(currentSword);
    }

    void DeactivateSword(int index)
    {
        swords[index].gameObject.SetActive(false);
    }

    void ActivateSword(int index)
    {
        SwordInfo sword = swords[index];

        sword.gameObject.SetActive(true);

        baseFollow.target = sword.bladeBase;
        tipFollow.target = sword.bladeTip;

        sliceTrail._colorGradient = sword.trailColor;

        audioSource.PlayOneShot(sword.drawSound);
    }

    public SwordInfo GetCurrentSword()
    {
        return swords[currentSword];
    }
}
