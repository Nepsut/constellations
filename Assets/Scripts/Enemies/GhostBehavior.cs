using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace constellations
{
    public class GhostBehavior : EnemyBase
    {
        #region variables

        [Header("Engine Variables")]
        private Rigidbody2D rb;
        private GameObject player;
        private PlayerAction playerAction;

        [Header("Constant Variables")]
        private const float awakeCheckFrequency = 0.5f;
        private const float seeDistance = 10f;
        private const float loseSightDistance = 15f;
        private const float stopMovingDistance = 0.3f;
        private const float acceleration = 1000f;
        private const float deceleration = 3f;
        private const float fastDeceleration = 10f;
        private const float moveSpeedTransitionTime = 3f;
        private const float knockbackStrength = 10f;
        private const float heavyHitMultiplier = 1.4f;
        private const float maxSpeed = 2f;
        private const float accelerationTime = 2f;
        public const float deathDuration = 1f;         //adjust depending on animation length

        [Header("Dynamic Variables")]
        private bool awake = false;
        private float allowedSpeed = 0;
        private float distance = 0;
        private Vector2 direction = Vector2.zero;
        private Coroutine lerpSpeed;

        #endregion

        #region standard methods

        void Awake()
        {
            //grab some references necessary later
            player = GameObject.FindGameObjectWithTag("Player");
            playerAction = player.GetComponent<PlayerAction>();
            rb = GetComponent<Rigidbody2D>();
        }

        // Start is called before the first frame update
        void Start()
        {
            //set allowespeed to max speed for now
            allowedSpeed = maxSpeed;
            StartCoroutine(AwakeCheck());
        }

        void FixedUpdate()
        {
            if (!awake) return;
            //if dead, return early
            if (isDead)
            {
                if (!isDying) StartCoroutine(Death());
                return;
            }

            //grab distance between player and this ghost, then also grab normalized direction for movement
            distance = Vector2.Distance(transform.position, player.transform.position);
            direction = (player.transform.position - transform.position).normalized;

            //if in movement range, move without drag, if outside, decelerate slowly, if too close, decelerate fast
            if (stopMovingDistance < distance && distance < seeDistance)
            {
                Movement();
                rb.drag = 0;
            }
            else if (distance > loseSightDistance)
            {
                rb.drag = deceleration;
            }
            else if (distance < stopMovingDistance) rb.drag = fastDeceleration;

            //self-explanatory
            if (doKnockback) Knockback();
        }

        #endregion

        private IEnumerator AwakeCheck()
        {
            while (Vector2.Distance(transform.position, player.transform.position) > seeDistance)
            {
                yield return new WaitForSeconds(awakeCheckFrequency);
            }
            awake = true;
        }

        protected override void Movement()
        {
            //addforce toward direction of player
            rb.AddForce(direction * acceleration * Time.deltaTime);
            //if speed over allowedSpeed, set speed to allowedSpeed
            if (Mathf.Abs(rb.velocity.x) > allowedSpeed || Mathf.Abs(rb.velocity.y) > allowedSpeed)
            {
                rb.velocity = new Vector2(direction.x, direction.y) * allowedSpeed;
            }
        }

        private void Knockback()
        {
            doKnockback = false;
            float trueKnockback;

            //calc true knockback for less messy calc later
            if (wasHeavyHit)
            {
                trueKnockback = (knockbackStrength + (playerAction.knockbackBuffs * PlayerAction.knockbackbBuffAmount)) *
                heavyHitMultiplier;
            }
            else 
            {
                trueKnockback = (knockbackStrength + (playerAction.knockbackBuffs * PlayerAction.knockbackbBuffAmount));
            }
            //add force toward direction opposite of player as impulse
            rb.AddForce(-direction * trueKnockback, ForceMode2D.Impulse);

            //stop and restart allowedSpeed calculation
            if (lerpSpeed != null ) StopCoroutine(lerpSpeed);
            lerpSpeed = null;
            lerpSpeed = StartCoroutine(MaxSpeedLerp());
        }

        //this lerp calculates max allowed speed, decreasing it slowly after a knockback
        private IEnumerator MaxSpeedLerp()
        {
            float takenTime = 0f;
            float startXSpeed = Mathf.Abs(rb.velocity.x);
            float startYSpeed = Mathf.Abs(rb.velocity.y);

            while (takenTime < moveSpeedTransitionTime)
            {
                if (Mathf.Abs(rb.velocity.x) < maxSpeed && Mathf.Abs(rb.velocity.y) < maxSpeed)
                {
                    allowedSpeed = maxSpeed;
                    break;
                }

                takenTime += Time.deltaTime;

                float lerpedMaxSpeedX = Mathf.Lerp(startXSpeed, maxSpeed, (takenTime / moveSpeedTransitionTime));
                float lerpedMaxSpeedY = Mathf.Lerp(startYSpeed, maxSpeed, (takenTime / moveSpeedTransitionTime));
                allowedSpeed = (lerpedMaxSpeedX > lerpedMaxSpeedY) ? lerpedMaxSpeedX : lerpedMaxSpeedY;
                yield return null;
            }
        }

        //this is ran on death
        protected override IEnumerator Death()
        {
            isDying = true;
            rb.drag = fastDeceleration;
            //play death animation
            //below is a placeholder
            this.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.red;

            yield return new WaitForSeconds(deathDuration);
            Destroy(this.gameObject);
        }
    }
}