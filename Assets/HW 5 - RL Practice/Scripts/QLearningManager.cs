using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlaneRL
{
    public class QLearningManager : MonoBehaviour
    {
        [HideInInspector] public static QLearningManager Instance { get; private set; }
        [HideInInspector] public Dictionary<(State, int), float> QValues { get; private set; }
        private int _episodes;
        [HideInInspector]
        public int Episodes
        {
            get { return _episodes; }
            set
            {
                references.episodesText.text = "Episodes: " + value;
                _episodes = value;
            }
        }
        private int _steps;
        [HideInInspector]
        public int Steps
        {
            get { return _steps; }
            set
            {
                references.stepsText.text = "Steps: " + value;
                _steps = value;
            }
        }

        [Header("Player Settings")]
        [Range(0, 360)]
        public int rotationRange = 45;

        [Space(5)]

        [Header("Hyperparameters")]
        [Range(0f, 1f)]
        public float alpha = 0.5f;
        [Range(0f, 1f)]
        public float gamma = 0.9f;
        [Range(0f, 1f)]
        public float epsilon = 0.3f;

        [Space(10)]

        [Header("References")]
        [SerializeField] private string outputDirectory = "/Data/";
        [SerializeField] private string outputFileName = "PlaneNavigation-QLearning";
        [SerializeField] private References references;

        private StreamWriter outStream;

        public void CompleteEpisode(float totalReward)
        {
            if (Application.isEditor)
            {
                outStream.WriteLine(Episodes + "," + totalReward);
                outStream.Flush();
            }
            Episodes++;
        }

        public void UpdateAlpha()
        {
            alpha = references.alphaSlider.value;
            references.alphaText.text = alpha.ToString("F2");
        }

        public void UpdateGamma()
        {
            gamma = references.gammaSlider.value;
            references.gammaText.text = gamma.ToString("F2");
        }

        public void UpdateEpsilon()
        {
            epsilon = references.epsilonSlider.value;
            references.epsilonText.text = epsilon.ToString("F2");
        }

        public void UpdateTimeScale()
        {
            Time.timeScale = references.timeScaleSlider.value;
            references.timeScaleText.text = Time.timeScale.ToString("F2");
        }

        public void UpdateQValue(State state, int action, float reward, State nextState)
        {
            float qValue = QValues.ContainsKey((state, action)) ? QValues[(state, action)] : 0f;
            float nextQValue = GetValue(nextState);
            float newQValue = qValue + alpha * (reward + gamma * nextQValue - qValue);
            QValues[(state, action)] = newQValue;
        }

        public float GetValue(State state)
        {
            float maxValue = float.MinValue;
            for (int i = 0; i <= rotationRange; i++)
            {
                float qValue = QValues.ContainsKey((state, i)) ? QValues[(state, i)] : 0f;
                if (qValue > maxValue)
                {
                    maxValue = qValue;
                }
            }
            return maxValue;
        }

        public int GetPolicy(State state)
        {
            float maxValue = float.MinValue;
            int bestAction = 0;
            for (int i = 0; i <= rotationRange; i++)
            {
                float qValue = QValues.ContainsKey((state, i)) ? QValues[(state, i)] : 0f;
                if (qValue > maxValue)
                {
                    maxValue = qValue;
                    bestAction = i;
                }
            }
            return bestAction;
        }

        public int GetAction(State state)
        {
            // account for noise
            if (Random.value < epsilon)
            {
                int value = Random.Range(-rotationRange, rotationRange + 1);
                Debug.Log(value);
                return value;
            }
            else
            {
                return GetPolicy(state);
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            QValues = new();
            Episodes = 0;
            Steps = 0;
        }

        private void Start()
        {
            // Initialize UI
            references.alphaText.text = alpha.ToString("F2");
            references.alphaSlider.value = alpha;
            references.gammaText.text = gamma.ToString("F2");
            references.gammaSlider.value = gamma;
            references.epsilonText.text = epsilon.ToString("F2");
            references.epsilonSlider.value = epsilon;
            references.timeScaleText.text = Time.timeScale.ToString("F2");
            references.timeScaleSlider.value = Time.timeScale;

            SetupOutputFile();
        }

        private void Update()
        {
            if (!Application.isPlaying && Application.isEditor)
            {
                // close file
                if (outStream != null)
                {
                    outStream.Close();
                    outStream = null;
                }
            }
        }

        private void SetupOutputFile()
        {
            if (!Application.isEditor)
                return;

            Directory.CreateDirectory(Application.streamingAssetsPath + outputDirectory);

            int version = 0;
            string filePath = Application.streamingAssetsPath + outputDirectory + outputFileName + "-"
                + version + ".csv";
            while (File.Exists(filePath))
            {
                version++;
                filePath = Application.streamingAssetsPath + outputDirectory + outputFileName + "-"
                    + version + ".csv";
            }

            File.WriteAllText(filePath, "Episode,Total Reward");
            outStream = new(filePath);
        }

        [System.Serializable]
        public struct References
        {
            [Header("Learning Rate")]
            public TMP_Text alphaText;
            public Slider alphaSlider;

            [Header("Discount Factor")]
            public TMP_Text gammaText;
            public Slider gammaSlider;

            [Header("Noise Rate")]
            public TMP_Text epsilonText;
            public Slider epsilonSlider;

            [Header("Time Scale")]
            public TMP_Text timeScaleText;
            public Slider timeScaleSlider;

            [Header("Other UI")]
            public TMP_Text stepsText;
            public TMP_Text episodesText;
        }
    }

    public struct State
    {
        public Vector2Int position { get; private set; }
        public int rotation { get; private set; }

        public State(Vector2Int position, int rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public override bool Equals(object obj)
        {
            if (obj is State other)
            {
                return position == other.position && rotation == other.rotation;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return position.GetHashCode() ^ rotation.GetHashCode();
        }
    }
}
