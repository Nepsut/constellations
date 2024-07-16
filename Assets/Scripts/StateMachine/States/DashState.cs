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
            //animator.Play(anim.name);
        }

        public override void Do()
        {
            if (time >= PlayerController.dashDecelerationIgnore)
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