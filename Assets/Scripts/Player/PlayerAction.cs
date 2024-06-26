using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace constellations
{
    public class PlayerAction : MonoBehaviour, IDataPersistence
    {
        #region variables

        [Header("Management Variables")]
        private bool attackEnabled = true;
        private bool screamEnabled = true;

        [Header("Engine Variables")]
        [SerializeField] private InputReader input;
        [SerializeField] private HitBoxController hitbox;

        [Header("Constant Action Variables")]
        private const int attackDamage = 20;
        private const int attackBuffAmount = 5;
        private const float attackSpeed = 10f;      //real attackspeed ends up being 10/attackSpeed
        private const float attackChargeTime = 1f;
        public const int knockbackbBuffAmount = 5;
        private const float screamMinDuration = 1.5f;
        private const float screamBufferTime = 0.1f;
        private const float meowTime = 0.2f;

        [Header("Dynamic Action Variables")]
        private int attackBuffs = 0;                //add 1 every time player's attack gets buffed
        [HideInInspector] public int knockbackBuffs { get; private set; } = 0;      //add 1 on knockback buff
        private int realDamage = 20;
        private float heavyAttackMult = 1.5f;
        private bool attackCooldown = false;
        private bool didAttack = false;
        private bool canHeavyAttack = false;
        private bool screaming = false;
        private bool screamKeyHeld = false;
        private bool meow = false;
        private Coroutine attackTypeCheck;
        private Coroutine scream;

        [Header("Interaction Variables")]
        private bool canInteractNPC = false;
        private bool canInteractObject = false;
        [HideInInspector] public bool didInteractObject = false;
        private GameObject interactingNPC;
        private GameObject interactingObject;
        private GameObject saveObject;


        #endregion

        #region standard methods

        private void Awake()
        {
            input.AttackEvent += HandleAttack;
            input.AttackCanceledEvent += HandleAttackCancel;
            input.ScreamEvent += HandleScream;
            input.ScreamCanceledEvent += HandleScreamCancel;
            input.MeowEvent += HandleMeow;
            input.InteractEvent += HandleInteract;
            input.InteractCanceledEvent += HandleInteractCancel;
        }

        private void Start()
        {
            realDamage = attackDamage + attackBuffs * attackBuffAmount;
        }

        //when entering a 2d trigger, check if it's from an NPC or an interactable object
        //this then lets player interact with said entity with the HandleInteract method
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision == null) return;
            if (collision.gameObject.CompareTag("NPC") || collision.gameObject.CompareTag("SavePoint"))
            {
                canInteractNPC = true;
                //save interactable NPC so we can easily call the Talk() method from it
                interactingNPC = collision.gameObject;
                //activate indicator to show this NPC can be interacted with
                if (interactingNPC.transform.GetChild(0).gameObject != null)
                {
                    interactingNPC.transform.GetChild(0).gameObject.SetActive(true);
                }
            }
            else if (collision.gameObject.CompareTag("Interactable"))
            {
                canInteractObject = true;
                //save interactable object so we can easily call the Interact() method from it
                interactingObject = collision.gameObject;
                if (interactingObject.transform.GetChild(0).gameObject != null)
                {
                    interactingObject.transform.GetChild(0).gameObject.SetActive(true);
                }
            }
            if (collision.gameObject.CompareTag("SavePoint"))
            {
                collision.gameObject.GetComponent<SavePoint>().usedSavepoint = true;
            }
        }

        //when leaving a 2d trigger, clear appropriate saved entity and disable indicators
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision == null) return;
            if (collision.gameObject.CompareTag("NPC") || collision.gameObject.CompareTag("SavePoint"))
            {
                canInteractNPC = false;
                if (interactingNPC == null) return;
                if (interactingNPC.transform.GetChild(0).gameObject != null)
                {
                    interactingNPC.transform.GetChild(0).gameObject.SetActive(false);
                }
                interactingNPC = null;
            }
            else if (collision.gameObject.CompareTag("Interactable"))
            {
                canInteractObject = false;
                if (interactingObject == null) return;
                if (interactingObject.transform.GetChild(0).gameObject != null)
                {
                    interactingObject.transform.GetChild(0).gameObject.SetActive(false);
                }
                interactingObject = null;
            }
            if (collision.gameObject.CompareTag("SavePoint"))
            {
                collision.gameObject.GetComponent<SavePoint>().usedSavepoint = false;
            }
        }

        #endregion

        #region input handlers

        private void HandleAttack()
        {
            if (!attackEnabled) return;
            if (!attackCooldown && !screaming)
            {
                Debug.Log("attack pressed");
                didAttack = true;
                attackTypeCheck = StartCoroutine(AttackTypeCheck());
            }
        }

        private void HandleAttackCancel()
        {
            if (!attackEnabled) return;
            if (didAttack)
            {
                Debug.Log("attack released");
                didAttack = false;
                StartCoroutine(Attack());
            }
        }

        private void HandleScream()
        {
            if (!screamEnabled) return;
            scream = StartCoroutine(Scream());
            screamKeyHeld = true;
        }

        private void HandleScreamCancel()
        {
            if (!screamEnabled) return;
            screamKeyHeld = false;
        }

        private void HandleMeow()
        {
            if (!meow) StartCoroutine(Meow());
        }

        //handles interaction based on data retrieved when entering trigger
        private void HandleInteract()
        {
            if (canInteractNPC && interactingNPC != null)
            {
                //change input mode so player movement is disabled during dialogue
                input.SetDialogue();
                //this calls the NPC's dialogue based in its INK story
                interactingNPC.GetComponent<NPCDialogue>().Talk();
            }
            else if (canInteractObject && interactingObject != null)
            {
                didInteractObject = true;
            }
        }

        private void HandleInteractCancel()
        {
            didInteractObject = false;
        }

        #endregion

        #region action methods

        private IEnumerator AttackTypeCheck()
        {
            //play animation for charge here
            yield return new WaitForSeconds(attackChargeTime);
            canHeavyAttack = true;
        }

        private IEnumerator Attack()
        {
            attackCooldown = true;
            if (canHeavyAttack) HeavyAttack(); 
            else NormalAttack();
            yield return new WaitForSeconds(10f / attackSpeed);
            attackCooldown = false;
        }

        private void NormalAttack()
        {
            StopCoroutine(attackTypeCheck);
            Debug.Log("normal attack done");
            if (hitbox.canAttackEnemy && hitbox.targetEnemy != null)
            {
                DealDamage(realDamage, false);
                Debug.Log(message: $"did normal attack on enemy for {realDamage} damage");
            }
        }

        private void HeavyAttack()
        {
            canHeavyAttack = false;
            Debug.Log("heavy attack done");
            if (hitbox.canAttackEnemy && hitbox.targetEnemy != null)
            {
                DealDamage(realDamage * heavyAttackMult, true);
                Debug.Log(message: $"did heavy attack on enemy for {realDamage * heavyAttackMult} damage");
            }
        }

        private void DealDamage(float t_damage, bool wasHeavy)
        {
            if (hitbox.targetEnemy.CompareTag("Ghost"))
            {
                hitbox.targetEnemy.GetComponentInParent<GhostBehavior>().TakeDamage(t_damage);
                hitbox.targetEnemy.GetComponentInParent<GhostBehavior>().wasHeavyHit = wasHeavy;

            }
            else if (hitbox.targetEnemy.CompareTag("Skeleton"))
            {
                hitbox.targetEnemy.GetComponentInParent<SkeletonBehavior>().TakeDamage(t_damage);
                hitbox.targetEnemy.GetComponentInParent<SkeletonBehavior>().wasHeavyHit = wasHeavy;
            }
            Debug.Log(message: $"did hit enemy for {t_damage} damage");
        }

        private IEnumerator Scream()
        {
            screaming = true;
            Debug.Log("screaming");
            //set screaming animation and sound here
            yield return new WaitForSeconds(screamMinDuration);
            while (screamKeyHeld)
            {
                yield return new WaitForSeconds(screamBufferTime);
            }
            Debug.Log("stopped screaming");
            //end screaming animation and sound here
            screaming = false;
        }

        private IEnumerator Meow()
        {
            meow = true;
            //meow animation & sound go here
            yield return new WaitForSeconds(meowTime);
            Debug.Log("meow");
            //and they end here
            meow = false;
        }

        #endregion

        #region data handling

        public void LoadData(GameData data)
        {
            this.attackEnabled = data.attackEnabled;
            this.screamEnabled = data.screamEnabled;
            this.attackBuffs = data.attackBuffs;
            this.knockbackBuffs = data.knockbackBuffs;
        }

        public void SaveData(ref GameData data)
        {
            data.attackEnabled = this.attackEnabled;
            data.screamEnabled = this.screamEnabled;
            data.attackBuffs = this.attackBuffs;
            data.knockbackBuffs = this.knockbackBuffs;
        }

        #endregion
    }
}