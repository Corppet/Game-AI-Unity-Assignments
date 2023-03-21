using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class Nurse : GAgent
    {
        protected override void Start()
        {
            base.Start();
            SubGoal s1 = new("treatPatient", 1, false);
            goals.Add(s1, 3);

            SubGoal s2 = new("rested", 1, false);
            goals.Add(s2, 4);

            SubGoal s3 = new("hasWandered", 1, false);
            goals.Add(s3, 5);

            Invoke(nameof(GetTired), Random.Range(10, 20));
        }

        void GetTired()
        {
            beliefs.ModifyState("exhausted", 0);
            Invoke(nameof(GetTired), Random.Range(10, 20));
        }
    }
}
