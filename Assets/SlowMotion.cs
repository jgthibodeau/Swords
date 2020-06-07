using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowMotion : MonoBehaviour
{
    private float originalTimeScale, originalFixedDeltaTime;

    public bool slowmoActive;
    public float slowmoTimescale = 0.25f;
    public float slowmoTimeChangeSpeed = 2f;

    public float maxSlowmoTime;
    public float remainingSlowmoTime;
    public float slowmoRechargeDelay;
    public float currentRechargeDelay;
    public float slowmoRechargeRate;

    void Start()
    {
        remainingSlowmoTime = maxSlowmoTime;
        originalTimeScale = Time.timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }
    
    void Update()
    {
        bool slowmoTriggered = Input.GetButtonDown("Slowmo");

        if (slowmoTriggered)
        {
            if (CanActivateSlowmo())
            {
                ActivateSlowmo();
            } else if (slowmoActive)
            {
                DeActivateSlowmo();
            }
        }
        
        if (slowmoActive)
        {
            UseSlowmo();
        }
        else
        {
            RegainSlowmo();
        }
    }

    bool CanActivateSlowmo()
    {
        return !slowmoActive && remainingSlowmoTime > 0;
    }

    void ActivateSlowmo()
    {
        slowmoActive = true;

        currentRechargeDelay = slowmoRechargeDelay;
    }

    void DeActivateSlowmo()
    {
        slowmoActive = false;
    }

    void UseSlowmo()
    {
        Time.timeScale = Mathf.Lerp(Time.timeScale, slowmoTimescale, Time.unscaledDeltaTime * slowmoTimeChangeSpeed);
        Time.fixedDeltaTime = Time.timeScale * originalFixedDeltaTime;

        remainingSlowmoTime -= Time.unscaledDeltaTime;
        if (remainingSlowmoTime < 0)
        {
            remainingSlowmoTime = 0;
            DeActivateSlowmo();
        }
    }

    void RegainSlowmo()
    {
        Time.timeScale = Mathf.Lerp(Time.timeScale, originalTimeScale, Time.unscaledDeltaTime * slowmoTimeChangeSpeed);
        Time.fixedDeltaTime = originalFixedDeltaTime;

        if (currentRechargeDelay > 0)
        {
            currentRechargeDelay -= Time.unscaledDeltaTime;
        }
        else
        {
            remainingSlowmoTime = Mathf.Min(remainingSlowmoTime + Time.unscaledDeltaTime * slowmoRechargeRate, maxSlowmoTime);
        }
    }
}
