using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth;

    public event System.Action OnDeath;

    protected float health; //lokal variabel, kommer inte synas i inspector
    protected bool dead;

    protected virtual void Start() //virtual för att den ska köras..
    {
        health = startingHealth;
    }

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection )
    {
        //Göra mer saker här med Hit variabel. typ particle effects
        TakeDamage(damage);
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        if(health <= 0 && !dead) //om hp och vi inte redan är död, Dör vi.
        {
            Die();
        }
    }

    public virtual void resetHealth()
    {
        health = startingHealth;
    }

    //Contextmenu gör så att man högerklicka i inspectorn på scriptet attached till gameobjekt, så körs funktionen under Contextmenu
    [ContextMenu("Die or self destruction")] 
    protected void Die()
    {
        dead = true;
        if(OnDeath != null)
        {
            OnDeath();
        }
        GameObject.Destroy(gameObject);
    }
}
