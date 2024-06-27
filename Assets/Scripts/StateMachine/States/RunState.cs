using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class RunState : State
    {
        [SerializeField] private AnimationClip anim;
        [SerializeField] private PlayerController controller;

        public override void Enter()
        {
            // core.animator.Play(anim.name);
        }

        public override void Do()
        {
            core.animator.speed = Helpers.Map(PlayerController.maxSpeed, 0, 1, 0, 1, true);

            if (!controller.running)
            isComplete = true;
        }

        public override void Exit()
        {
            core.animator.speed = 1;
        }
    }
}