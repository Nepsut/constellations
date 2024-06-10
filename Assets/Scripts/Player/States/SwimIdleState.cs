using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "SwimIdleState")]
    public class SwimIdleState : State
    {
        public override void Enter()
        {
            // animator.Play("SwimIdle");
        }

        public override void Do()
        {
            if (!input.swimming || input.horizontal != 0) isComplete = true;
        }
    }
}