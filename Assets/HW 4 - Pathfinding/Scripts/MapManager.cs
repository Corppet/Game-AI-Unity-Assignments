using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

namespace Pathfinding
{
    public class MapManager : MonoBehaviour
    {
        [HideInInspector] public static MapManager instance { get; private set; }

        [HideInInspector] public MapData mapData;
        [HideInInspector] public List<Node> nodes;

        public MapTiles mapTiles;
        public GraphSettings graphSettings;

        [SerializeField] private TextAsset[] mapFiles;

        [Space(5)]

        [SerializeField] private float playerOffset = 2f;

        [Space(5)]

        [SerializeField] private TMP_Dropdown mapDropdown;

        public void UpdateMap()
        {
            GameManager.instance.player.StopAllCoroutines();
            graphSettings.pathRenderer.positionCount = 0;
            ProcessMapFile(mapFiles[mapDropdown.value]);
            LoadMap();
        }

        public void ProcessMapFile(TextAsset file)
        {
            /*
             * map format
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

            // clear the graph
            foreach (Transform child in graphSettings.parent)
            {
                Destroy(child.gameObject);
                nodes.Clear();
            }
            foreach (Transform child in graphSettings.searchParent)
            {
                Destroy(child.gameObject);
            }

            // load the map data
            for (int z = 0; z < mapData.height; z++)
            {
                bool foundStartPos = false; // used to set the first ground tile as the starting position

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
                            // invisible tile
                            Instantiate(mapTiles.invisibleBlock, obstaclePos, Quaternion.identity, 
                                obstacle.transform);
                            break;
                        case '.':
                            // ground tile
                            Instantiate(mapTiles.platform, groundPos, Quaternion.identity, ground.transform);
                            if (!foundStartPos)
                            {
                                // set the start position
                                Vector3 playerPos = groundPos;
                                playerPos.y += playerOffset;
                                GameManager.instance.startingPosition = playerPos;
                                foundStartPos = true;
                            }
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
            //mapTiles.groundSurface.BuildNavMesh();
            BuildGraph();
            GameManager.instance.ResetPlayer();
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

            nodes = new List<Node>();
        }

        private void Start()
        {
            SetupDropdown();

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

        private void SetupDropdown()
        {
            // add the map names to the dropdown
            List<string> mapNames = new List<string>();
            foreach (TextAsset file in mapFiles)
            {
                mapNames.Add(file.name);
            }

            // set the dropdown options
            mapDropdown.ClearOptions();
            mapDropdown.AddOptions(mapNames);
            mapDropdown.value = 0;
        }

        private bool isInBounds(int x, int z)
        {
            return x >= 0 && x < mapData.width && z >= 0 && z < mapData.height;
        }

        private bool isWalkable(int x, int z)
        {
            return mapData.data[z][x] == '.';
        }

        private void BuildGraph()
        {
            // instantiate a node at every corner of the graph
            for (int z = 0; z < mapData.height; z++)
            {
                for (int x = 0; x < mapData.width; x++)
                {
                    // ignore if not ground
                    if (mapData.data[z][x] != '.')
                        continue;

                    // check for diagonally adjacent obstacle
                    bool hasDiagAdj = false;
                    for (int i = -1; i <= 1; i += 2)
                    {
                        for (int j = -1; j <= 1; j += 2)
                        {
                            if (isInBounds(x + i, z + j) && !isWalkable(x + i, z + j))
                            {
                                hasDiagAdj = true;
                                break;
                            }
                        }
                    }

                    // check for orthogonally adjacent obstacle
                    bool hasOrthAdj = false;
                    for (int i = -1; i <= 1; i += 2)
                    {
                        if (isInBounds(x, z + i) && !isWalkable(x, z + i))
                        {
                            hasOrthAdj = true;
                            break;
                        }
                        if (isInBounds(x + i, z) && !isWalkable(x + i, z))
                        {
                            hasOrthAdj = true;
                            break;
                        }
                    }

                    // the tile is a corner if there is diagonally-adjacent but not orthogonally-adjacent obstacles
                    if (hasDiagAdj && !hasOrthAdj) 
                    {
                        Tilemap ground = mapTiles.groundMap;
                        Vector3 nodePos = new Vector3(
                            ground.tileAnchor.x + x, 
                            mapTiles.cellSize.y + ground.tileAnchor.y + graphSettings.nodeOffset,
                            ground.tileAnchor.z + z);
                        GameObject node = Instantiate(graphSettings.nodePrefab, nodePos, Quaternion.identity, graphSettings.parent);
                        nodes.Add(node.GetComponent<Node>());
                    }
                }
            }

            // build edges
            foreach (Node node in nodes)
            {
                node.FindNeighbors();
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
        public GameObject invisibleBlock;
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

    [System.Serializable]
    public struct GraphSettings
    {
        [Tooltip("The offset of the node from the ground.")]
        public float nodeOffset;
        [Tooltip("The offset of the path from the ground.")]
        public float pathOffset;

        [Space(5)]

        public GameObject nodePrefab;
        public LineRenderer pathRenderer;
        public Transform parent;

        [Space(5)]

        public GameObject searchEdgePrefab;
        public Transform searchParent;
    }
}
