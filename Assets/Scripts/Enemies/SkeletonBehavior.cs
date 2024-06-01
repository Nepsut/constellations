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
        private Rigidbody2D rb;
        private GameObject player;
        private PlayerAction playerAction;
        [SerializeField] private LayerMask climbable;
        [SerializeField] private LayerMask ground;

        [Header("Constant Variables")]
        private const float seeDistance = 10f;
        private const float loseSightDistance = 15f;
        private const float stopMovingDistance = 0.3f;
        private const float acceleration = 5000f;
        private const float deceleration = 8f;
        private const float moveSpeedTransitionTime = 3f;
        private const float knockbackStrength = 10f;
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
        private bool seesPlayer = false;
        private bool touchingPlayer = false;
        private float allowedSpeed = 0;
        private float distance = 0;
        private Vector2 direction = Vector2.zero;
        private bool jumpOnCD = false;
        private bool climbing = false;
        private Coroutine lerpSpeed;

        #endregion

        #region standard methods

        void Awake()
        {
            //grab some references necessary later
            player = GameObject.FindGameObjectWithTag("Player");
            playerAction = player.GetComponent<PlayerAction>();
            rb = GetComponent<Rigidbody2D>();

            BoxCollider2D box = gameObject.GetComponentInChildren<BoxCollider2D>();

            //set raycast sizes based on collider sizes to ensure enemy is scaleable
            jumpRaycastBox = new Vector2(box.size.x, box.size.y + 0.1f);
            climbRaycastBox = new Vector2(box.size.x + 0.04f, box.size.y);
        }

        // Start is called before the first frame update
        void Start()
        {
            //set allowespeed to max speed for now
            allowedSpeed = maxSpeed;
        }

        void FixedUpdate()
        {
            //if dead, return early
            if (isDead)
            {
                if (!isDying) StartCoroutine(Death());
                return;
            }

            //grab distance between player and this skeleton, then also grab normalized direction for movement
            distance = Vector2.Distance(transform.position, player.transform.position);
            direction = (player.transform.position - transform.position).normalized;

            //if in movement range, move without drag, if outside, decelerate slowly, if too close, decelerate fast
            if (stopMovingDistance < distance && distance < seeDistance)
            {
                seesPlayer = true;
                rb.drag = 0;
                Movement();
            }
            else if (distance > loseSightDistance)
            {
                seesPlayer = false;
                rb.drag = deceleration;
            }
            else if (distance < stopMovingDistance) rb.drag = deceleration;

            //various checks ran to see if jumping is a good option
            if (!jumpOnCD && rb.velocity.x == 0 && seesPlayer && CanJump() && !CanClimb() && !touchingPlayer)
            StartCoroutine(Jump());

            //more checks to see if climbing is possible and a good option
            else if (seesPlayer && CanClimb())
            {
                climbing = true;
                Climb();
            }

            else if (seesPlayer && !CanClimb())
            {
                climbing = false;
            }

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

        protected override void Movement()
        {
            if (!climbing)
            {
                //addforce toward direction of player
                rb.AddForce(Vector2.right * direction.x * acceleration * Time.deltaTime);
                //if speed over allowedSpeed, set speed to allowedSpeed
                if (Mathf.Abs(rb.velocity.x) > allowedSpeed)
                {
                    rb.velocity = new Vector2(direction.x * allowedSpeed, rb.velocity.y);
                }
            }
            else
            {
                rb.velocity = new Vector2(direction.x * 0.1f, rb.velocity.y);
            }
        }

        private void Climb()
        {
            Debug.Log("climbing");
            //addforce upwards
            rb.AddForce(Vector2.up * acceleration * climbMult * Time.deltaTime);
            //if speed over allowedSpeed, set speed to allowedSpeed
            if (Mathf.Abs(rb.velocity.y) > allowedSpeed * climbMult)
            {
                rb.velocity = new Vector2(rb.velocity.x, allowedSpeed * climbMult);
            }
        }

        private IEnumerator Jump()
        {
            jumpOnCD = true;
            yield return new WaitForSeconds(jumpDelay);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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
            float startSpeed = Mathf.Abs(rb.velocity.x);

            while (takenTime < moveSpeedTransitionTime)
            {
                if (Mathf.Abs(rb.velocity.x) < maxSpeed)
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
        private bool CanClimb()
        {
            RaycastHit2D hitRight = Physics2D.BoxCast(transform.position, climbRaycastBox, 0f, Vector2.right, 0.1f, climbable);
            RaycastHit2D hitLeft = Physics2D.BoxCast(transform.position, climbRaycastBox, 0f, Vector2.left, 0.1f, climbable);
            if (hitRight || hitLeft)
            {
                rb.gravityScale = 0;
                return true;
            }
            else
            {
                rb.gravityScale = 1;
                return false;
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
            rb.drag = deceleration;
            //play death animation
            //below is a placeholder
            this.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.red;

            yield return new WaitForSeconds(deathDuration);
            Destroy(this.gameObject);
        }
    }
}