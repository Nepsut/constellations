using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "SwimState")]
    public class SwimState : State
    {
        public override void Enter()
        {
            // animator.Play("Swim");
        }

        public override void Do()
        {
            if (!input.swimming || Mathf.Abs(input.horizontal) < 0.1f) isComplete = true;
        }
    }
}