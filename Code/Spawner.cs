using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool devMode;
    public Wave[] waves;
    public Enemy enemy;

    LivingEntity playerEntity;
    Transform playerTransform;

    Wave currentWave;
    int currentWaveNumber;
    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;

    MapGenerator map;

    public float TimeBetweenCampCheck = 2;
    float campThresholdDistance = 1.5f;
    float nextCampCheckTime;
    bool isCamping;
    Vector3 campPositionOld;

    bool isDisabled;

    public event System.Action<int> OnNewWave;

    void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerTransform = playerEntity.transform;

        nextCampCheckTime = TimeBetweenCampCheck + Time.time;
        campPositionOld = playerTransform.position;

        playerEntity.OnDeath += OnPlayerDeath;

        map = FindObjectOfType<MapGenerator>();

        NextWave();
    }

    void Update()
    {
        if(!isDisabled)
        {
            //kollar om spelare campar nedan
            if(Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + TimeBetweenCampCheck;

                isCamping = (Vector3.Distance(playerTransform.position, campPositionOld) < campThresholdDistance);
                campPositionOld = playerTransform.position;
            }

            //Spawnar in enemys
            if((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                //StartCoroutine(SpawnEnemy()); Funkar också men I unity kan man bar stoppa Coroutines om de är definerade med en string, som nedan:
                StartCoroutine("SpawnEnemy");
            }
        }
        if(devMode)
        {
            if(Input.GetKeyDown(KeyCode.Return))
            {
                StopCoroutine("SpawnEnemy");
                foreach(Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    GameObject.Destroy(enemy.gameObject);
                }
                NextWave();
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform spawnTile = map.GetRandomOpenTile();
        if(isCamping)
        {
            spawnTile = map.GetTileFromPosition(playerTransform.position);
        }
        Material tileMaterial = spawnTile.GetComponent<Renderer>().material;
        //Har vi vita tiles, kan vi göra initialColor = Color.white , för ibland blir tilematerial.color = flashcolorlerp
        Color intitialColor = map.tilePrefab.GetComponent<Renderer>().sharedMaterial.color;
        Color FlashColor = Color.red;
        float spawnTimer = 0;

        while(spawnTimer < spawnDelay)
        {
            tileMaterial.color = Color.Lerp(intitialColor, FlashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath; //Sätter eventet OnDeath som finns i Livingentity till funktionen OnEnemyDeath

        spawnedEnemy.SetStats(currentWave.moveSpeed, currentWave.enemyDamage, currentWave.enemyHealth, currentWave.skinColor);
    }

    void OnEnemyDeath()
    {
        enemiesRemainingAlive --;
        if(enemiesRemainingAlive == 0) //Om alla mobs har dött i vågen så sätter den igång nästa våg.
        {
            NextWave();
        }
    }

    void OnPlayerDeath()
    {
        isDisabled = true;
    }

    void ResetPlayerPosition()
    {
        playerTransform.position = map.GetTileFromPosition(Vector3.zero).position + new Vector3(0, 2, 0); // + Vector3.up * 2 funkar också
    }

    //Sätter ny våg nedan
    void NextWave()
    {
        currentWaveNumber ++;
        
        if(currentWaveNumber - 1 < waves.Length) // Om alla waves tar slut i arrayen så krashar det inte. 
        {
            currentWave = waves[currentWaveNumber - 1];

            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;

            if(OnNewWave != null)
            {
                OnNewWave(currentWaveNumber);
            }
            ResetPlayerPosition();
            playerEntity.resetHealth();
        }
    }

    [System.Serializable] // nu syns den i inspectorn (arrayen)
    public class Wave
    {
        public bool infinite;
        public int enemyCount;
        public float timeBetweenSpawns;

        public float moveSpeed;
        public int enemyDamage;
        public float enemyHealth;
        public Color skinColor;
    }
}
