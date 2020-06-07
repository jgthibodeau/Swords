using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    public bool isBroken;
    public GameObject unBroken;
    public GameObject broken;

    public float minBreakForce;

    public PlayAudio breakAudio;
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        unBroken.SetActive(true);
        broken.SetActive(false);

        BreakableChild bc = unBroken.GetComponent<BreakableChild>();
        if (bc == null)
        {
            bc = unBroken.AddComponent<BreakableChild>();
        }
        bc.breakableParent = this;
    }

    //void Update()
    //{
    //    if (isBroken)
    //    {
    //        //monitor children until they stop moving
    //    }
    //}

    public void Break(float breakForce, GameObject go)
    {
        //Debug.Log(isBroken + " " + breakForce);
        if (isBroken)
        {
            return;
        }

        SwordObject swordObject = go.GetComponentInParent<SwordObject>();
        bool swordSwing = swordObject != null && swordObject.sword.swinging;

        if (swordSwing || breakForce > minBreakForce) { //or go is sword
            broken.transform.position = unBroken.transform.position;
            broken.transform.rotation = unBroken.transform.rotation;

            unBroken.SetActive(false);
            broken.SetActive(true);

            isBroken = true;

            breakAudio.Play(audioSource);
        }
    }
}
