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

            SubGoal s2 = new("isTreated", 1, true);
            goals.Add(s2, 5);

            SubGoal s4 = new("hasWandered", 1, true);
            goals.Add(s4, 6);

            SubGoal s3 = new("isHome", 1, true);
            goals.Add(s3, 7);
        }
    }
}
