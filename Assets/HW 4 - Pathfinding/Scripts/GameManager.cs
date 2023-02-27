using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class GameManager : MonoBehaviour
    {
        [HideInInspector] public static GameManager instance { get; private set; }

        [HideInInspector] public Vector3 startingPosition;

        [SerializeField] private GameObject playerPrefab;

        private GameObject player;

        public void ResetPlayer()
        {
            if (player != null)
            {
                Destroy(player);
            }

            player = Instantiate(playerPrefab, startingPosition, Quaternion.identity);
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
    }
}
