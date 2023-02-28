using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Pathfinding
{
    public class GameManager : MonoBehaviour
    {
        [HideInInspector] public static GameManager instance { get; private set; }

        [HideInInspector] public Vector3 startingPosition;
        [HideInInspector] public PlayerController player;

        [Header("Player Settings")]
        [Range(0f, 100f)]
        public float speed = 10f;

        [Header("Pathfinding Settings")]
        [Range(0f, 1f)]
        public float heuristicWeight = .5f;
        [Range(0f, 5f)]
        public float searchDelay = .25f;
        public HeuristicType heuristic;

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private UIReferences uiReferences;

        public void UpdateHeuristic()
        {
            heuristic = (HeuristicType)uiReferences.heuristicDropdown.value;
            player.heuristic = heuristic;
        }

        public void UpdateHeuristicWeight()
        {
            heuristicWeight = uiReferences.heuristicWeightSlider.value;
            uiReferences.heuristicWeightText.text = "Heuristic Weight: " + heuristicWeight.ToString("F2");
            player.heuristicWeight = heuristicWeight;
        }

        public void UpdateSearchDelay()
        {
            searchDelay = uiReferences.searchDelaySlider.value;
            uiReferences.searchDelayText.text = "Search Delay: " + searchDelay.ToString("F2");
            player.searchDelay = searchDelay;
        }

        public void ResetPlayer()
        {
            if (player != null)
            {
                Destroy(player.gameObject);
            }

            player = Instantiate(playerPrefab, startingPosition, Quaternion.identity)
                .GetComponent<PlayerController>();
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
            SetupDropdown();

            uiReferences.heuristicWeightText.text = "Heuristic Weight: " + heuristicWeight.ToString("F2");
            uiReferences.heuristicWeightSlider.value = heuristicWeight;

            uiReferences.searchDelayText.text = "Search Delay: " + searchDelay.ToString("F2");
            uiReferences.searchDelaySlider.value = searchDelay;
        }

        private void SetupDropdown()
        {
            TMP_Dropdown dropdown = uiReferences.heuristicDropdown;

            dropdown.ClearOptions();
            List<string> options = new List<string>();
            foreach (HeuristicType mode in System.Enum.GetValues(typeof(HeuristicType)))
            {
                options.Add(mode.ToString());
            }
            dropdown.AddOptions(options);
            dropdown.value = (int)heuristic;
        }
    }

    [System.Serializable]
    public struct UIReferences
    {
        public TMP_Dropdown heuristicDropdown;

        [Space(5)]

        public TMP_Text heuristicWeightText;
        public Slider heuristicWeightSlider;

        [Space(5)]

        public TMP_Text searchDelayText;
        public Slider searchDelaySlider;
    }
}
