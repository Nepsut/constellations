using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class PatrolState : State
    {
        public State navigate;
        public State idle;

        public Transform anchor1;
        public Transform anchor2;

        private void NextDestination()
        {
            float randomPoint = Random.Range(anchor1.position.x, anchor2.position.x);
            navigate.destination = new Vector2(randomPoint, core.transform.position.y);
            machine.Set(navigate, true);
        }

        public override void Enter()
        {
            NextDestination();
        }

        public override void Do()
        {
            if (machine.state == navigate)
            {
                if (navigate.isComplete)
                {
                    machine.Set(idle, true);
                    rb2d.velocity = new Vector2(0, rb2d.velocity.y);
                }

            }
            else
            {
                if (machine.state.time > 1f)
                {
                    NextDestination();
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}