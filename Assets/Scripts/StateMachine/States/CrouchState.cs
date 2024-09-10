using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class CrouchState : State
    {
        [SerializeField] private AnimationClip anim;

        public override void Enter()
        {
            core.animator.Play(anim.name); 
        }

        public override void Do()
        {
            core.animator.speed = Helpers.Map(StateMachineCore.maxSpeed * StateMachineCore.crouchSpeedMult,
            0, 1, 0, 1, true);

            if (!core.crouching)
            isComplete = true;
        }

        public override void Exit()
        {
            core.animator.speed = 1;
        }
    }
}