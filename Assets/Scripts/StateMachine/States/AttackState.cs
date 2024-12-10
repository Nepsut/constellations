using UnityEngine;

namespace constellations
{
    public class AttackState : State
    {
        [field: SerializeField] public AnimationClip anim { get; private set; }
        private float timer = 0;

        public override void Enter()
        {
            animator.Play(anim.name);
            timer = 0;
        }

        public override void Do()
        {
            timer += Time.deltaTime;

            if (timer >= anim.length)
            {
                core.attacking = false;
                isComplete = true;
                core.timeSinceLastAttack = 0;
            }
        }

        public override void Exit()
        {
            core.animator.speed = 1;
        }
    }
}