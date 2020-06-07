using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float moveSpeed, maxShakeTime;

    public float shakeAmount, shakeTime;

    public void Shake(float amount, float time)
    {
        shakeAmount = Mathf.Max(amount, shakeAmount);
        shakeTime = Mathf.Clamp(shakeTime + time, 0, maxShakeTime);
    }

    void Update()
    {
        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
            Shake();
        } else
        {
            shakeAmount = 0;
            transform.localPosition = Vector3.zero;
        }
    }

    void Shake()
    {
        Vector3 position = Vector3.zero;

        position.x += Random.Range(-shakeAmount, shakeAmount) * Time.deltaTime;
        position.y += Random.Range(-shakeAmount, shakeAmount) * Time.deltaTime;

        position = Vector3.ClampMagnitude(position, shakeAmount);

        transform.localPosition = position;
    }
}
