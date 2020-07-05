using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DampCamera2D : MonoBehaviour
{

    public Transform target;
    public float percent;

    Vector3 targetOffset;

    void Start()
    {
        targetOffset = transform.position - target.position;
    }
    void Update()
    {   
        if(target != null)
        {
            transform.position = Vector3.Lerp(transform.position, target.position + targetOffset, percent);
        }
    }
}