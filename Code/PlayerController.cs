﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Rigidbody))]
public class PlayerController : MonoBehaviour 
{
    Vector3 jump;
    Vector3 velocity;
    Rigidbody myRigidBody;

    bool doJump;
    

    void Start() 
    { 
        myRigidBody = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 _velocity) 
    {
        velocity = _velocity;
    }

    public void LookAt(Vector3 lookPoint) 
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }

    public void Jump(Vector3 _jump, bool _doJump)
    {
        jump = _jump;
        doJump = _doJump;
    }

    void FixedUpdate() 
    {
        myRigidBody.MovePosition(myRigidBody.position + velocity * Time.fixedDeltaTime);
        if(doJump)
        {
            myRigidBody.AddForce(jump, ForceMode.Impulse);
            doJump = false;
        }   
    }
} 
