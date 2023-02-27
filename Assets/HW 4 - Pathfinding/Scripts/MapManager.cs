using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [HideInInspector] public static MapManager instance { get; private set; }

    [SerializeField] private TextAsset[] mapFiles;
    [SerializeField] private MapTiles mapTiles;

    private MapData mapData;

    public void ProcessMapFile(TextAsset file)
    {
        /*
         * .map format
         * 
         * type <type>
         * height <height>
         * width <width>
         * map
         * <tiles>
         */

        string[] lines = file?.text.Split('\n');
        if (lines.Length < 5)
        {
            Debug.LogError("Map file does not have enough data");
            return;
        }

        // remove '\r' chars
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Replace("\r", "");
        }

        // get the map type
        string[] words = lines[0]?.Split(' ');
        if (words?.Length >= 2 && words[0] == "type")
        {
            mapData.type = words[1];
        }
        else
        {
            Debug.LogError("Map file is missing type");
            return;
        }

        // get the map height
        words = lines[1]?.Split(' ');
        if (words?.Length >= 2 && words[0] == "height")
        {
            mapData.height = int.Parse(words[1]);
        }
        else
        {
            Debug.LogError("Map file is missing height");
            return;
        }

        // get the map width
        words = lines[2]?.Split(' ');
        if (words?.Length >= 2 && words[0] == "width")
        {
            mapData.width = int.Parse(words[1]);
        }
        else
        {
            Debug.LogError("Map file is missing width");
            return;
        }

        // get the map data
        if (lines[3] != "map")
        {
            Debug.LogError("Map file is missing map data");
            return;
        }

        // rest of the lines are map data
        mapData.data = new List<string>();
        for (int i = 4; i < lines.Length; i++)
        {
            mapData.data.Add(lines[i]);
        }
    }

    public void LoadMap()
    {
        Tilemap ground = mapTiles.groundMap;
        Tilemap obstacle = mapTiles.obstacleMap;

        // clear the tilemaps
        foreach (Transform child in ground.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in obstacle.transform)
        {
            Destroy(child.gameObject);
        }

        // load the map data
        for (int z = 0; z < mapData.height; z++)
        {
            for (int x = 0; x < mapData.width; x++)
            {
                // get the tile
                char tile = mapData.data[z][x];

                // get the tile position
                Vector3 groundPos = new Vector3(
                    x * mapTiles.cellSize.x + ground.tileAnchor.x + ground.transform.position.x,
                    ground.tileAnchor.y + ground.transform.position.y,
                    z * mapTiles.cellSize.z + ground.tileAnchor.z + ground.transform.position.z);

                Vector3 obstaclePos = new Vector3(
                    x * mapTiles.cellSize.x + obstacle.tileAnchor.x + obstacle.transform.position.x,
                    obstacle.tileAnchor.y + obstacle.transform.position.y,
                    z * mapTiles.cellSize.z + obstacle.tileAnchor.z + obstacle.transform.position.z);

                // set the tile
                switch (tile)
                {
                    case '@':
                        // empty tile
                        break;
                    case '.':
                        // ground tile
                        Instantiate(mapTiles.platform, groundPos, Quaternion.identity, ground.transform);
                        break;
                    case 'T':
                        // obstacle tile
                        Instantiate(mapTiles.platform, groundPos, Quaternion.identity, ground.transform);
                        Instantiate(mapTiles.obstacle, obstaclePos, Quaternion.identity, obstacle.transform);
                        break;
                    default:
                        Debug.LogError("Invalid tile type + \'" + tile + "\'");
                        break;
                }
            }
        }

        // update the navmesh
        mapTiles.groundSurface.BuildNavMesh();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (mapFiles.Length > 0)
        {
            ProcessMapFile(mapFiles[0]);
            LoadMap();
        }
        else
        {
            Debug.LogError("No map files found");
        }
    }
}

[System.Serializable]
public struct MapTiles
{
    public Vector3 cellSize;

    [Space(5)]

    public GameObject platform;
    public Tilemap groundMap;
    public NavMeshSurface groundSurface;

    [Space(5)]

    public GameObject obstacle;
    public Tilemap obstacleMap;
}

[System.Serializable]
public struct MapData
{
    public string type;
    public int height;
    public int width;
    public List<string> data;
}
