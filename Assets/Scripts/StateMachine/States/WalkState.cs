using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class WalkState : State
    {
        [SerializeField] private AnimationClip anim;
        [SerializeField] private float maxSpeed;

        private void Start()
        {
            //needs smarter way to fetch a more appropriate value like allowedSpeed or just rb.velocity,
            //which one? why? should be moved to update when this changes as well
            if (gameObject.CompareTag("Player"))
            {
                maxSpeed = StateMachineCore.maxSpeed;
            }
        }

        public override void Enter()
        {
            core.animator.Play(anim.name);
        }

        public override void Do()
        {
            core.animator.speed = Helpers.Map(maxSpeed, 0, 1, 0, 1, true);

            if (!core.groundSensor.grounded)
            isComplete = true;
        }

        public override void Exit()
        {
            core.animator.speed = 1;
        }
    }
}