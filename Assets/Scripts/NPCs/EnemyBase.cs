using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace constellations
{
    public abstract class EnemyBase : StateMachineCore, IDamageable
    {
        [Header("Enemy Base Variables")]
        protected GameObject player;
        protected PlayerController playerController;
        [SerializeField] protected SpriteRenderer enemySprite;
        [SerializeField] private float health = 100f;
        public bool isDead { get; private set; } = false;
        protected bool isDying = false;
        protected bool doKnockback = false;
        public bool wasHeavyHit { get; set; } = false;
        private float invulnerableDuration = 0;
        protected Coroutine damagedStateTimer;
        protected AudioSource audioSource;

        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            //grab some references necessary later
            player = GameObject.FindGameObjectWithTag("Player");
            playerController = player.GetComponent<PlayerController>();
        }

        public virtual void TakeDamage(float _damage, float _invulDuration)
        {
            if (invulnerableDuration > 0) return;
            invulnerableDuration = _invulDuration;
            damaged = true;     //THIS NEEDS TO BE SET TO FALSAE WITHIN INHERITING CLASS!!!
            health -= _damage;
            doKnockback = true;
            if (health <= 0)
            {
                isDead = true;
            }
            damagedStateTimer = StartCoroutine(EndDamagedState());
        }

        protected virtual void Movement()
        {
            //flip sprite according to movement direction
            if (rb2d.velocity.x > 0.1f && !facingRight) SpriteFlip();
            else if (rb2d.velocity.x < -0.1f && facingRight) SpriteFlip();
        }

        protected virtual void Update()
        {
            if (invulnerableDuration > 0)
            invulnerableDuration -= Time.deltaTime;
        }

        protected virtual IEnumerator Death()
        {
            isDying = true;
            yield return null;
        }

        protected abstract IEnumerator EndDamagedState();

        //simplified version of sprite flipper
        private void SpriteFlip()
        {
            if (facingRight)
            {
                enemySprite.flipX = false;
                facingRight = !facingRight;
            }
            else
            {
                enemySprite.flipX = true;
                facingRight = !facingRight;
            }
        }
    }
}
