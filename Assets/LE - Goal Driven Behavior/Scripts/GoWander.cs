using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class GoWander : GAction
    {
        public override bool PrePerform()
        {
            return true;
        }

        public override bool PostPerform()
        {
            return true;
        }
    }
}
