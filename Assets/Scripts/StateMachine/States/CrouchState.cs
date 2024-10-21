using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class CrouchState : State
    {
        [SerializeField] private PlayerController controller;
        [SerializeField] private AnimationClip anim;

        public override void Enter()
        {
            core.animator.Play(anim.name); 
        }

        public override void Do()
        {
            core.animator.speed = Helpers.Map(PlayerController.maxSpeed * PlayerController.crouchSpeedMult,
            0, 1, 0, 1, true);

            if (!controller.crouching)
            isComplete = true;
        }

        public override void Exit()
        {
            core.animator.speed = 1;
        }
    }
}