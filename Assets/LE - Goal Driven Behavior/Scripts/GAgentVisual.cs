using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    [ExecuteInEditMode]
    public class GAgentVisual : MonoBehaviour
    {
        public GAgent thisAgent;

        void Start()
        {
            thisAgent = GetComponent<GAgent>();
        }

        void Update()
        {

        }
    }
}
