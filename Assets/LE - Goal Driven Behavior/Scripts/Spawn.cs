using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class Spawn : MonoBehaviour
    {
        public GameObject patientPrefab;
        public int numPatients;

        private void Start()
        {
            for (int i = 0; i < numPatients; i++)
            {
                Instantiate(patientPrefab, transform.position, Quaternion.identity);
            }

            Invoke(nameof(SpawnPatient), Random.Range(7, 10));
        }

        private void SpawnPatient()
        {
            Instantiate(patientPrefab, transform.position, Quaternion.identity);
            Invoke(nameof(SpawnPatient), Random.Range(7, 10));
        }
    }
}
