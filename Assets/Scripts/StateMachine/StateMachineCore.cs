using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

namespace constellations
{
    public abstract class StateMachineCore : ClimbableSensor
    {
        public Rigidbody2D rb2d;
        public Animator animator;
        public StateMachine machine;
        public GroundSensor groundSensor;
        public bool facingRight { get; protected set; } = true;
        [SerializeField] protected Vector2 size = Vector2.one;
        public Vector2 offset = Vector2.zero;

        public void SetupInstances()
        {
            machine = new StateMachine();
            State[] allChildStates = GetComponentsInChildren<State>();
            foreach (State state in allChildStates)
            {
                state.SetCore(this);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            if (facingRight)
            {
                Gizmos.DrawWireCube(transform.position + new Vector3(offset.x, offset.y, 0), size);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position + new Vector3(-offset.x, offset.y, 0), size);
            }
        }
    }
}