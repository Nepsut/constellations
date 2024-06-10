using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "IdleState")]
    public class IdleState : State
    {
        public override void Enter()
        {
            animator.Play("Idle");
        }

        public override void Do()
        {
            if (!input.grounded || input.horizontal != 0) isComplete = true;
        }
    }
}