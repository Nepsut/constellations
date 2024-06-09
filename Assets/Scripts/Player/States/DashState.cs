using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "DashState")]
    public class DashState : State
    {
        float time;
        public override void Enter()
        {
            // animator.Play("Dash");
        }

        public override void Do()
        {
            time += Time.deltaTime;

            if (time >= PlayerController.dashDecelerationIgnore)
            {
                isComplete = true;
            }
        }

        public override void Exit()
        {
            animator.speed = 1;
        }
    }
}