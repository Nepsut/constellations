using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace constellations
{
    public abstract class EnemyBase : StateMachineCore, IDamageable
    {
        [Header("Enemy Base Variables")]
        [SerializeField] private float health = 100f;
        public bool isDead { get; private set; } = false;
        protected bool isDying = false;
        protected bool doKnockback = false;
        [HideInInspector] public bool wasHeavyHit = false;

        public void TakeDamage(float damage)
        {
            health -= damage;
            doKnockback = true;
            if (health <= 0)
            {
                isDead = true;
            }
        }

        protected abstract void Movement();

        protected abstract IEnumerator Death();
    }
}
