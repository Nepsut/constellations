using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "CrouchIdleState")]
    public class CrouchIdleState : State
    {
        public override void Enter()
        {
            // animator.Play("CrouchIdle");
        }

        public override void Do()
        {
            if (!input.crouching || !input.grounded || input.horizontal != 0) isComplete = true;
        }
    }
}