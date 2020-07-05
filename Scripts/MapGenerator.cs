
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform mapFloor;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;

    public Vector2 maxMapSize;

    [Range(0, 1)] // Clampar outlinePercent från 0 till 1.
    public float outlinePercet;
    public float tileSize;

    List<Coords> allTileCoords;

    Queue<Coords> shuffledTiledCoords;
    Queue<Coords> shuffledOpenTiledCoords; // Tiles utan obstacle

    Transform[,] tileMap;

    Map currentMap;

    void Awake()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave; //subrscribar eventet
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }

    public void GenerateMap()
    {
        currentMap = maps[mapIndex];

        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];

        System.Random psuedoRandomGenerator = new System.Random(currentMap.seed);

        //Genererar Coords
        allTileCoords = new List<Coords>();
        for(int x = 0; x < currentMap.mapSize.x; x++)
        {
            for(int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coords(x, y));
            }
        }

        shuffledTiledCoords = new Queue<Coords>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed)); // Till för GetRandomCoords() för just den här mapen
        
        //skapar map holder objektet.
        string holderName = "Generated Map";
        //Förstör mapen om den finns nedan:
        if(transform.Find(holderName))
        {  
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;

        mapHolder.parent = transform;

        //Skapar Tiles till mapen
        for(int x = 0; x < currentMap.mapSize.x; x++)
        {
            for(int y = 0; y < currentMap.mapSize.y; y++)
            {
                //kalkylera postion för att spawna tilen till världen
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;

                newTile.localScale = Vector3.one * (1 - outlinePercet) * tileSize; // sätter outlinen i tilesen. dvs kan göra dem mindre på sin position
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile;
            }
        }

        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y]; //en 2d-bool

        //nedan för att sätta ut hinder på mapen
        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent); //sätter ut hinder i procent i förhållande till storlek på mapen.
        int currentObstacleCount = 0;
        List<Coords> allOpenCoords = new List<Coords>(allTileCoords);
        for(int i = 0; i < obstacleCount; i++)
        {
            Coords randomCoords = GetRandomCoords();
            obstacleMap[randomCoords.x, randomCoords.y] = true;
            currentObstacleCount ++;

            if(randomCoords != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)psuedoRandomGenerator.NextDouble()); //Unity jobbar nästan bara med floats, så konverterar double till float
                Vector3 obstaclePosition = CoordToPosition(randomCoords.x, randomCoords.y);

                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3((1 - outlinePercet) * tileSize, obstacleHeight, (1 - outlinePercet) * tileSize);

                //Sätter färgen på materialet på obstacle nedan
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = randomCoords.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foreGroundColor, currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoords);
            }
            else
            {
                obstacleMap[randomCoords.x, randomCoords.y] = false;
                currentObstacleCount --;
            }
        }

        shuffledOpenTiledCoords = new Queue<Coords>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));

        //sätter ut en navmeshvägg så AI ej kan göra pathfinder igenom den.
        //sätter ut den i kanterna av mapen Mellan currentMap.mapSize och maxcurrentMap.mapSize.
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
        mapFloor.localScale = new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize); // x och axis bara eftersom mapFloor är roterad 90 grader redan
    }

    // Metoden / funktionen nedan kollar om tile 0,0 har tillgång till något av de yttre tile'sen utan att det är någon obstacle ivägen
    // dvs att metoden kollar tiles som grannar och ser om det kan sättas en obstacle där.
    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coords> queue = new Queue<Coords>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int acessibleTileCount = 1;

        while(queue.Count > 0)
        {
            Coords tile = queue.Dequeue();

            for(int x = -1; x <= 1; x++)
            {
                for(int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if(x == 0 || y == 0)
                    {
                        if(neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >=0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            if(!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coords(neighbourX, neighbourY));
                                acessibleTileCount ++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == acessibleTileCount;
    }

    //sätter kordinater till en position på mapen, med lite justeringar så 0,0 i tilen hamnar i mitten av tilen -> lokalt dvs.
    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
    }

    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f); //Mathf.RoundToInt är mer exakt för avrundning än att casta till "(int)"
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        
        //För att inte få ett array index error ifall vi får en kordinat som inte finns på mapen. SÅ behöver vi Clampa x och y
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
        return tileMap[x, y];
    }

    public Coords GetRandomCoords()
    {
        Coords randomCoords = shuffledTiledCoords.Dequeue();
        shuffledTiledCoords.Enqueue(randomCoords);
        return randomCoords;
    }

    public Transform GetRandomOpenTile()
    {
        Coords randomCoords = shuffledOpenTiledCoords.Dequeue();
        shuffledOpenTiledCoords.Enqueue(randomCoords);
        return tileMap[randomCoords.x, randomCoords.y];
    }

    //en struktur för kordinater på mapen.
    [System.Serializable] //visar den i inspectorn i Unity
    public struct Coords
    {
        public int x;
        public int y;

        public Coords(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coords c1, Coords c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coords c1, Coords c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Coords coords)
            {
                return this == coords;
            }
        return false;
        }

        public override int GetHashCode() => new{x, y}.GetHashCode();
        /*{
            return new{x,y}.GetHashCode();
        }*/ 
    }

    [System.Serializable] //visar den i inspectorn i Unity
    public class Map
    {
        public Coords mapSize;

        public int seed;

        [Range(0, 1)]
        public float obstaclePercent;
        public float minObstacleHeight;
        public float maxObstacleHeight;

        public Color foreGroundColor;
        public Color backgroundColor;

        public Coords mapCenter
        {
            get
            {
                return new Coords(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}