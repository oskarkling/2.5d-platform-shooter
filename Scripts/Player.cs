using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{

    public float moveSpeed = 5;
    public float jumpForce = 2.0f;

    public Crosshairs crosshair;
    
    Vector3 jump;
    
    bool onGround = true;
    bool doJump = true;

    Camera viewCamera;
    PlayerController controller;
    GunController gunController;

    protected override void Start()  //override för att kunna köra start från LivingEntity och start från Player.
    {
        base.Start(); // för att kunna köra start från LivingEntity och start från Player.
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        
        jump = new Vector3(0.0f, 2.0f, 0.0f) * jumpForce;
        //Vector3 jump = new Vector3(0, 2.0f, 0);
        
    }

    void OnCollisionStay()
    {
        onGround = true;
    }

    void OnCollisionExit()
    {
        onGround = false;
    }
    
    void Update() 
    {
        // Movement input
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"),0 , Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        // Jump Input
        if(Input.GetKeyDown(KeyCode.Space) && onGround)
        {
            controller.Jump(jump, doJump);
            onGround = false;
        }

        // Look input
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.gunPosHeight);
        float rayDistance;

        if(groundPlane.Raycast(ray, out rayDistance)) 
        {
            Vector3 point = ray.GetPoint(rayDistance);
            // Debug.DrawLine(ray.origin, point, Color.red);
            controller.LookAt(point);
            crosshair.transform.position = point;
            crosshair.crosshairDetectTarget(ray, rayDistance);
            
            if((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1)
            {
                gunController.Aim(point);
            }
        }

        // Weapon input
        if(Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }

        if(Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }
    }

    //Inga väggar runt Mapen, så faller ner = death
    void LateUpdate()
    {
        if(transform.position.y < -5f)
        {
            Die();
        }
    }
}
