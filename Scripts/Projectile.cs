using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour 
{
    //Lägger till ett lager för kollisioner med kulan. Sätter Enemy som ett lager till kulan i unity.
    public LayerMask collisionMask;
    public LayerMask floorCollision;
    public LayerMask obstacleCollision;
    //public Color trailColor;

    float speed = 10;
    float damage = 1;
    float lifeTimeBullet = 3;
    float skinWidth = 0.1f; //ifall Enemy rör sig så missar kulan collidern så vi lägger till ett extra lager.

    void Start()
    {
        Destroy(gameObject, lifeTimeBullet);

        //Gör en array för collisions om Enemy är för nära spelaren.
        //För att kunna skjuta Enemy.
        Collider[] initCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionMask);
        if(initCollisions.Length > 0)
        {
            OnHitObject(initCollisions[0], transform.position);
        }

        //GetComponent<TrailRenderer>().startColor = trailColor;
        //GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor);
    }

    public void SetSpeed(float newSpeed) 
    {
        speed = newSpeed;
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    void Update() 
    {
        float bulletMoveDistance = speed * Time.deltaTime;
        CheckCollisions(bulletMoveDistance);
        transform.Translate(Vector3.forward * bulletMoveDistance);
    }

    //Kollar om kulan träffar Collidern
    void CheckCollisions(float bulletMoveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit rayHit;

        if(Physics.Raycast(ray, out rayHit, bulletMoveDistance + skinWidth, collisionMask + floorCollision + obstacleCollision, QueryTriggerInteraction.Collide))
        {
            OnHitObject(rayHit.collider, rayHit.point);
        }
    }

    void OnHitObject(Collider collider, Vector3 hitPoint) //tar in arrayen om Enemy är för nära positionen från pipan på vapnet.
    {
        IDamageable damageableObject = collider.GetComponent<IDamageable>();
        if(damageableObject != null)
        {
            damageableObject.TakeHit(damage, hitPoint, transform.forward);
        }
        GameObject.Destroy(gameObject);
    }
}