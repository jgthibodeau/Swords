using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineWaver : MonoBehaviour
{
    public float waver;
    public float waverSpeed;
    public int materialIndex;
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    
    void Update()
    {
        waver -= Time.deltaTime * waverSpeed;
        if (waver < 0f)
        {
            waver += 0.5f;
        }
        
        lineRenderer.sharedMaterials[materialIndex].SetTextureOffset(Shader.PropertyToID("_MainTex"), new Vector2(waver, 0f));
    }
}
