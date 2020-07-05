using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]

public class Enemy : LivingEntity
{
    public enum State {Idle, Chasing, Attacking}; //Behöver göra states så inte UpdatePath() förstör attacken
    State currentState;

    public ParticleSystem deathEffect;

    UnityEngine.AI.NavMeshAgent pathfinder;
    Transform target;

    LivingEntity targetEntity;

    Material skinMaterial;
    Color originalColor;

    float attackDistanceThreshold = .5f;
    float timeBetweenAttacks = 1;
    float nextAttackTime;
    float damage = 1;

    float myCollisionRadius;
    float TargetCollisionRadius;

    bool hasTarget;
    
    //"GetComponent" är bättre att ha i Awake, För i det här fallet kommer SetStatsmetoden kallas före Start() från Spawnerklassen.
    void Awake() //Awake körs först i klassen om klassen skulle kallas av en annan klass. Dvs Awake() körs före Start() i Unity
    {
        pathfinder = GetComponent<UnityEngine.AI.NavMeshAgent>();

        if(GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();

            // Så Enemy transform inte kommer in i target transform position.
            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            TargetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
    }
    protected override void Start()
    {
        base.Start(); //kallar på start i LivingEntity

        if(hasTarget)
        {
            currentState = State.Chasing; // Enemy jagar som default
            targetEntity.OnDeath += OnTargetDeath;
            StartCoroutine(UpdatePath());
        }
    }

    public void SetStats(float moveSpeed, int enemyDamage, float enemyHealth, Color skinColor)
    {
        pathfinder.speed = moveSpeed;
        damage = enemyDamage;
        startingHealth = enemyHealth;
        skinMaterial = GetComponent<Renderer>().material; //Hämtar renderern och materialet
        skinMaterial.color = skinColor;
        originalColor = skinMaterial.color;
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if(damage >= health)
        {
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.startLifetime);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    void OnTargetDeath()
    {
        hasTarget = false; //Om spelaren dör så finns inget target för Enemy.
        currentState = State.Idle;
    }

    //Kolla i update om Enemy ska attackera
    void Update()
    {
        if(hasTarget)
        {
            if(Time.time > nextAttackTime) // tid mellan attackerna
            {
                // vi kan använda Vector3.Distance() här. men då behöver cpu göra roten ur på alla beräkningar så de blir för dålig performance,
                //så det blir en vanlig uträkning för att få get distance squared.
                float squareDistanceToTarget = (target.position - transform.position).sqrMagnitude;

                if(squareDistanceToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + TargetCollisionRadius, 2)) //Det här är om Enemy är i range att attackera
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    //om vi Enemy var inom range så gör vi en attack genom att kalla på Coroutinen Attack()
                    StartCoroutine(Attack());
                }
            }
        }
    }

    //attackfunktionen som är en Corountine.
    IEnumerator Attack()
    {
        currentState = State.Attacking;
        //När while loopen körs och enemy ska göra sin attack så kan vi inte fortsätta använda UpdatePath()
        //Det hade förstört animationen, därav= 
        pathfinder.enabled = false;


        Vector3 originalPosition = transform.position;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - directionToTarget * (myCollisionRadius);

        float attackSpeed = 3; // känns som om attackspeed ska va public
        float percent = 0;

        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;
         
        while (percent <= 1)
        {
            if(percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
            percent += Time.deltaTime * attackSpeed;
            //koden nedan visar hur Enemy ställer sig på startpositionnen igen med y = 4(-x^2 + x)
            float interpolation = (-Mathf.Pow(percent,2) + percent) * 4; //(-percent * percent + percent) * 4. Math.Pow är upphöjt.
            
            //När interpolation = 0 får vi originalpostion, när interpolation = 1 får vi attackPosition, när interpolation = 0 igen så ör vi tillbaka på originalposition
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);


            yield return null; //för att göra så koden körs varje frame i whileloopen. Annars kör whileloopen hela tiden vilket ger dålig performance.
        }
        skinMaterial.color = originalColor;
        currentState = State.Chasing; // När vi är klara med attacken så forstätter Enemy jaga.
        pathfinder.enabled = true; // sätter igång pathfindern i UpdatePath() igen
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;

        while(hasTarget) //sålänge vi Enemy hasTarget = true :)
        {
            if(currentState == State.Chasing) //kollar vilket state för annars skiter vi att göra pathfinding.
            {   
                //Behöver direction to target för att Enemy inte ska pathfinda exakt till transform.target
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - directionToTarget * (myCollisionRadius + TargetCollisionRadius + attackDistanceThreshold / 2);
                if(!dead)
                {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
