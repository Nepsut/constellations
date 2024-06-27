using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace constellations
{
    public class SkeletonBehavior : EnemyBase
    {
        #region variables

        [Header("Engine Variables")]
        [SerializeField] private GameObject player;
        private PlayerAction playerAction;
        [SerializeField] private LayerMask ground;

        [Header("Constant Variables")]
        private const float awakeCheckFrequency = 0.5f;
        private const float seeDistance = 10f;
        private const float loseSightDistance = 15f;
        private const float stopMovingDistance = 0.3f;
        private const float acceleration = 5000f;
        private const float deceleration = 8f;
        private const float moveSpeedTransitionTime = 3f;
        private const float knockbackStrength = 25f;
        private const float heavyHitMultiplier = 1.4f;
        private const float maxSpeed = 2f;
        private const float climbMult = 0.4f;
        private const float accelerationTime = 2f;
        private const float jumpForce = 65f;
        private const float jumpCooldown = 1.2f;
        private const float jumpDelay = 0.8f;
        private const float climbThreshold = 1f;
        private Vector2 jumpRaycastBox;
        private Vector2 climbRaycastBox;
        public const float deathDuration = 1f;         //adjust depending on animation length

        [Header("Dynamic Variables")]
        private bool awake = false;
        private bool seesPlayer = false;
        private bool touchingPlayer = false;
        private float allowedSpeed = 0;
        private float distance = 0;
        private Vector2 direction = Vector2.zero;
        private bool jumpOnCD = false;
        private Coroutine lerpSpeed;

        #endregion

        #region standard methods

        void Awake()
        {
            //grab some references necessary later
            playerAction = player.GetComponent<PlayerAction>();
            rb2d = GetComponent<Rigidbody2D>();

            BoxCollider2D box = gameObject.GetComponentInChildren<BoxCollider2D>();

            //set raycast sizes based on collider sizes to ensure enemy is scaleable
            jumpRaycastBox = new Vector2(box.size.x, box.size.y + 0.1f);
            climbRaycastBox = new Vector2(box.size.x + 0.04f, box.size.y - 0.04f);
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

            //change this to proper midpoint calculation when sprite added
            CheckWall(transform.position + new Vector3(offset.x, offset.y, 0), size);

            //grab distance between player and this skeleton, then also grab normalized direction for movement
            distance = Vector2.Distance(transform.position, player.transform.position);
            direction = (player.transform.position - transform.position).normalized;

            //if in movement range, move without drag, if outside, decelerate slowly, if too close, decelerate fast
            if (stopMovingDistance < distance && distance < seeDistance)
            {
                seesPlayer = true;
                rb2d.drag = 0;
                Movement();
            }
            else if (distance > loseSightDistance)
            {
                seesPlayer = false;
                rb2d.drag = deceleration;
            }
            else if (distance < stopMovingDistance) rb2d.drag = deceleration;

            IsClimbing(direction.x);

            //various checks ran to see if jumping is a good option
            if (!jumpOnCD && rb2d.velocity.x == 0 && seesPlayer && CanJump() && canClimb < 0 && !touchingPlayer)
            StartCoroutine(Jump());

            //more checks to see if climbing is possible and a good option
            else if (seesPlayer && climbing)
            {
                Climb();
            }

            if (climbing) rb2d.gravityScale = 0;
            else rb2d.gravityScale = 1;

            //self-explanatory
            if (doKnockback) Knockback();
        }

        private void OnCollisionEnter2D(Collision2D collider)
        {
            if (collider != null)
            {
                if (collider.gameObject.CompareTag("Player")) touchingPlayer = true;
            }
        }

        private void OnCollisionExit2D(Collision2D collider)
        {
            if (collider != null)
            {
                if (collider.gameObject.CompareTag("Player")) touchingPlayer = false;
            }
        }

        #endregion

        #region behavior and checks

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
            if (!climbing)
            {
                //addforce toward direction of player
                rb2d.AddForce(Vector2.right * direction.x * acceleration * Time.deltaTime);
                //if speed over allowedSpeed, set speed to allowedSpeed
                if (Mathf.Abs(rb2d.velocity.x) > allowedSpeed)
                {
                    rb2d.velocity = new Vector2(direction.x * allowedSpeed, rb2d.velocity.y);
                }
            }
            else
            {
                rb2d.velocity = new Vector2(direction.x * 0.1f, rb2d.velocity.y);
            }
        }

        private void Climb()
        {
            //addforce upwards
            rb2d.AddForce(Vector2.up * acceleration * climbMult * Time.deltaTime);
            //if speed over allowedSpeed, set speed to allowedSpeed
            if (Mathf.Abs(rb2d.velocity.y) > allowedSpeed * climbMult)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, allowedSpeed * climbMult);
            }
        }

        private IEnumerator Jump()
        {
            jumpOnCD = true;
            yield return new WaitForSeconds(jumpDelay);
            rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            yield return new WaitForSeconds(jumpCooldown);
            jumpOnCD = false;
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
            rb2d.AddForce(-direction * trueKnockback, ForceMode2D.Impulse);

            //stop and restart allowedSpeed calculation
            if (lerpSpeed != null ) StopCoroutine(lerpSpeed);
            lerpSpeed = null;
            lerpSpeed = StartCoroutine(MaxSpeedLerp());
        }

        //this lerp calculates max allowed speed, decreasing it slowly after a knockback
        private IEnumerator MaxSpeedLerp()
        {
            float takenTime = 0f;
            float startSpeed = Mathf.Abs(rb2d.velocity.x);

            while (takenTime < moveSpeedTransitionTime)
            {
                if (Mathf.Abs(rb2d.velocity.x) < maxSpeed)
                {
                    allowedSpeed = maxSpeed;
                    break;
                }

                takenTime += Time.deltaTime;

                float lerpedMaxSpeed = Mathf.Lerp(startSpeed, maxSpeed, (takenTime / moveSpeedTransitionTime));
                allowedSpeed = lerpedMaxSpeed;
                yield return null;
            }
        }

        //same simple climb raycast tech as player exceot on a boolean as walljumps are not necessary
        private bool CanJump()
        {
            RaycastHit2D hit = Physics2D.BoxCast(transform.position, jumpRaycastBox, 0f, Vector2.down, 0.1f, ground);
            if (hit) return true;
            else return false;
        }

        //this is ran on death
        protected override IEnumerator Death()
        {
            isDying = true;
            rb2d.drag = deceleration;
            //play death animation
            //below is a placeholder
            this.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.red;

            yield return new WaitForSeconds(deathDuration);
            Destroy(this.gameObject);
        }

        #endregion
    }
}