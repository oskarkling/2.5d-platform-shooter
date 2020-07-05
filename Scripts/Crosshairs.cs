using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{
    public LayerMask targetMask;
    public Color crosshairHighlightColor;
    public SpriteRenderer crosshair;
    Color originalCrosshairColor;

    void Start()
    {
        Cursor.visible = false;
        originalCrosshairColor = crosshair.color;
    } 

    /*void Update()
    {
        //det var  inte skönt med
        //transform.Rotate(Vector3.forward * -40 * Time.deltaTime);
    }*/

    public void crosshairDetectTarget(Ray ray, float rayDistance)
    {
        if(Physics.Raycast(ray, rayDistance, targetMask))
        {
            crosshair.color = crosshairHighlightColor;
        }
        else
        {
            crosshair.color = originalCrosshairColor;
        }
    }
}
