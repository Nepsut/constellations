using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class SwimState : State
    {
        [SerializeField] private AnimationClip anim;

        public override void Enter()
        {
            // animator.Play(anim.name);
        }

        public override void Do()
        {
            if (!core.swimming) isComplete = true;
        }
    }
}