using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class DashState : State
    {
        [SerializeField] private AnimationClip anim;
        public override void Enter()
        {
            core.animator.Play(anim.name); 
        }

        public override void Do()
        {
            core.animator.speed = 1;

            if (time >= PlayerController.dashAnimDuration)
            {
                isComplete = true;
            }
        }

        public override void Exit()
        {
            core.animator.speed = 1;
        }
    }
}