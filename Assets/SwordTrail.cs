using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTrail : MonoBehaviour
{
    private bool swinging;
    public Sword sword;
    public MeleeWeaponTrail[] trails;
    void LateUpdate()
    {
        if (swinging != sword.swinging)
        {
            swinging = sword.swinging;
            //UpdateTrails();


            foreach (MeleeWeaponTrail trail in trails)
            {
                trail.Emit = swinging;
            }
        }
    }

    private void UpdateTrails()
    {
        if (swinging)
        {
            foreach (MeleeWeaponTrail trail in trails)
            {
                trail.Emit = true;
            }
        } else
        {
            foreach (MeleeWeaponTrail trail in trails)
            {
                StartCoroutine(Disable(trail));
            }
        }
    }

    private IEnumerator Disable(MeleeWeaponTrail trail)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        trail.Emit = swinging;
    }
}
