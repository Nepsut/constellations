using System;
using System.Collections;
using UnityEngine;

namespace constellations
{
    public class GhostBehavior : EnemyBase
    {
        #region variables

        [Header("Engine Variables")]
        [SerializeField] private AudioClip damageSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private Color damagedColor;

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
        public new const float maxSpeed = 2f;
        public const float deathDuration = 1f;         //adjust depending on animation length
        private const int damage = 20;

        [Header("Dynamic Variables")]
        private bool awake = false;
        private float allowedSpeed = 0;
        private float distance = 0;
        private Vector2 direction = Vector2.zero;
        private Coroutine lerpSpeed;
        private bool touchingPlayer = false;
        private bool playedDamageSound = false;

        [Header("States")]
        [SerializeField] private State idleState;
        [SerializeField] private DamagedState damagedState;
        [SerializeField] private State deathState;


        #endregion

        #region standard methods

        protected override void Awake()
        {
            base.Awake();

            SetupInstances();   //setup state machine
        }

        // Start is called before the first frame update
        void Start()
        {
            //set allowedspeed to max speed for now
            allowedSpeed = maxSpeed;
            StartCoroutine(AwakeCheck());
        }

        void FixedUpdate()
        {
            if (!awake) return;

            SelectState();
            machine.state.Do();
            
            //if dead, return early
            if (isDead)
            {
                if (!isDying) StartCoroutine(Death());
                return;
            }

            //grab distance between player and this ghost, then also grab normalized direction for movement
            distance = Vector2.Distance(transform.position, playerController.centerPosition);
            direction = (playerController.centerPosition - transform.position).normalized;

            //if in movement range, move without drag, if outside, decelerate slowly, if too close, decelerate fast
            if (stopMovingDistance < distance && distance < seeDistance)
            {
                Movement();
                rb2d.drag = 0;
            }
            else if (distance > loseSightDistance)
            {
                rb2d.drag = deceleration;
            }
            else if (distance < stopMovingDistance) rb2d.drag = fastDeceleration;

            //self-explanatory
            if (doKnockback) Knockback();
        }

        protected override void Update()
        {
            base.Update();
            
            if (touchingPlayer && !playerController.invulnerable && !playerController.invulnerableOverride && !isDead)
            {
                playerController.DamagePlayer(damage);
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider == null) return;

            if (collider.gameObject.CompareTag("Player")) touchingPlayer = true;

        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider != null)
            {
                if (collider.gameObject.CompareTag("Player")) touchingPlayer = false;
            }
        }

        #endregion

        private IEnumerator AwakeCheck()
        {
            while (Vector2.Distance(transform.position, playerController.centerPosition) > seeDistance)
            {
                yield return new WaitForSeconds(awakeCheckFrequency);
            }
            awake = true;
        }

        protected override void Movement()
        {
            base.Movement();    //this runs sprite flipper

            //addforce toward direction of player
            rb2d.AddForce(direction * acceleration * Time.deltaTime);
            //if speed over allowedSpeed, set speed to allowedSpeed
            if (Mathf.Abs(rb2d.velocity.x) > allowedSpeed || Mathf.Abs(rb2d.velocity.y) > allowedSpeed)
            {
                rb2d.velocity = new Vector2(direction.x, direction.y) * allowedSpeed;
            }
        }

        public override void TakeDamage(float _damage, float _invulDuration)
        {
            base.TakeDamage(_damage, _invulDuration);

            if (!isDead)
            {
                if (!playedDamageSound)
                audioSource.PlayOneShot(damageSound);
                playedDamageSound = true;
                enemySprite.color = damagedColor;
            }
        }

        private void Knockback()
        {
            doKnockback = false;
            float trueKnockback;

            //calc true knockback for less messy calc later
            if (wasHeavyHit)
            {
                trueKnockback = (knockbackStrength + (playerController.knockbackBuffs * PlayerController.knockbackbBuffAmount)) *
                heavyHitMultiplier;
            }
            else 
            {
                trueKnockback = (knockbackStrength + (playerController.knockbackBuffs * PlayerController.knockbackbBuffAmount));
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
            float startXSpeed = Mathf.Abs(rb2d.velocity.x);
            float startYSpeed = Mathf.Abs(rb2d.velocity.y);

            while (takenTime < moveSpeedTransitionTime)
            {
                if (Mathf.Abs(rb2d.velocity.x) < maxSpeed && Mathf.Abs(rb2d.velocity.y) < maxSpeed)
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
            StartCoroutine(base.Death());

            audioSource.PlayOneShot(deathSound);
            rb2d.drag = fastDeceleration;
            //play death animation
            //below is a placeholder
            enemySprite.color = Color.red;

            yield return new WaitForSeconds(deathSound.length);
            Destroy(this.gameObject);
        }

        protected override IEnumerator EndDamagedState()
        {
            yield return new WaitForSeconds(damagedState.animLength);
            damaged = false;
            enemySprite.color = Color.white;
            playedDamageSound = false;
        }

        private void SelectState()
        {
            if (isDead)
            {
                machine.Set(deathState);
            }
            else if (!damaged)
            {
                machine.Set(idleState);
            }
            else
            {
                machine.Set(damagedState);
            }
        }
    }
}