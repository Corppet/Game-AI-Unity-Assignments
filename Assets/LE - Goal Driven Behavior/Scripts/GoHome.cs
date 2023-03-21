using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class GoHome : GAction
    {
        public override bool PrePerform()
        {
            return true;
        }

        public override bool PostPerform()
        {
            Destroy(gameObject);
            return true;
        }
    }
}
