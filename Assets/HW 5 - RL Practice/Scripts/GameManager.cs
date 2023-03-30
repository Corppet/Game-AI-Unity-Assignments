using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PlaneRL
{
    public class GameManager : MonoBehaviour
    {
        [HideInInspector] public static GameManager Instance { get; private set; }

        public Wall[] walls;

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
        }
    }
}
