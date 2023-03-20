using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class Patient : GAgent
    {
        protected override void Start()
        {
            base.Start();
            SubGoal s1 = new("isWaiting", 1, true);
            goals.Add(s1, 3);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
