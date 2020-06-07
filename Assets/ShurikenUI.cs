using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShurikenUI : MonoBehaviour
{
    public Throw throwScript;

    public Image fillImage;
    public TMP_Text text;
    
    // Update is called once per frame
    void Update()
    {
        float fillAmount = (float)throwScript.throwsLeft / (float)throwScript.maxThrows;

        float rechargeSize = 1 / (float)throwScript.maxThrows;

        fillAmount += rechargeSize * (1f - throwScript.currentThrowDelay / throwScript.throwRegainDelay);

        fillImage.fillAmount = fillAmount;
        text.text = "" + throwScript.throwsLeft;
    }
}
