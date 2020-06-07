using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kill : MonoBehaviour
{
    public float minLife, maxLife;
    public bool dieOnStart = true;

    // Start is called before the first frame update
    void Start()
    {
        if (dieOnStart)
        {
            TriggerDeath();
        }
    }

    void TriggerDeath()
    {
        Invoke("Die", Random.Range(minLife, maxLife));
    }
    
    void Die()
    {
        GameObject.Destroy(this.gameObject);
    }

    void OnCollisionEnter(Collision c)
    {
        CancelInvoke();
        TriggerDeath();
    }
}
