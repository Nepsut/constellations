using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class CrouchIdleState : State
    {
        [SerializeField] private PlayerController controller;
        [SerializeField] private AnimationClip anim;
        
        public override void Enter()
        {
            // animator.Play(anim.name);
        }

        public override void Do()
        {
            if (!controller.crouching) isComplete = true;
        }
    }
}