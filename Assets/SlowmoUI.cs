using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlowmoUI : MonoBehaviour
{
    public SlowMotion slowMotion;

    public Image fillImage;
    public TMP_Text text;

    // Update is called once per frame
    void Update()
    {
        float fillAmount = slowMotion.remainingSlowmoTime / slowMotion.maxSlowmoTime;
        fillImage.fillAmount = fillAmount;
        //text.text = Mathf.RoundToInt(fillAmount * 100) + "%";
        text.text = slowMotion.remainingSlowmoTime.ToString("F1") + "s";
    }
}
