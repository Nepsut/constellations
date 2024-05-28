using System.Collections;
using UnityEngine;

namespace constellations
{
    public class PlayerAction : MonoBehaviour
    {
        #region variables

        [Header("Engine Variables")]
        [SerializeField] private InputReader input;
        [SerializeField] private HitBoxController hitbox;

        [Header("Constant Attack Variables")]
        private const int attackDamage = 20;
        private const int attackBuffAmount = 5;
        private const float attackSpeed = 10f;      //real attackspeed ends up being 10/attackSpeed
        private const float attackChargeTime = 0.5f;

        [Header("Dynamic Attack Variables")]
        private int attackBuffs = 0;                //add 1 every time player's attack gets buffed
        private int realDamage = 20;
        private float heavyAttackMult = 1.5f;
        private bool attackCooldown = false;
        private bool didAttack = false;
        private bool canHeavyAttack = false;

        private Coroutine attackTypeCheck;

        [Header("Interaction Variables")]
        private bool canInteractNPC = false;
        private bool canInteractObject = false;
        [HideInInspector] public bool didInteractObject = false;
        private GameObject interactingNPC;
        private GameObject interactingObject;


        #endregion

        #region standard methods

        private void Awake()
        {
            input.AttackEvent += HandleAttack;
            input.AttackCanceledEvent += HandleAttackCancel;
            input.ScreamEvent += HandleScream;
            input.ScreamCanceledEvent += HandleScreamCancel;
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
            if (collision.gameObject.CompareTag("NPC"))
            {
                canInteractNPC = true;
                //save interactable NPC so we can easily call the Talk() method from it
                interactingNPC = collision.gameObject;
                //activate indicator to show this NPC can be interacted with
                interactingNPC.transform.GetChild(0).gameObject.SetActive(true);
            }
            else if (collision.gameObject.CompareTag("Interactable"))
            {
                canInteractObject = true;
                //save interactable object so we can easily call the Interact() method from it (TODO)
                interactingObject = collision.gameObject;
                interactingObject.transform.GetChild(0).gameObject.SetActive(true);
            }
        }

        //when leaving a 2d trigger, clear appropriate saved entity and disable indicators
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision == null) return;
            if (collision.gameObject.CompareTag("NPC"))
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
        }

        #endregion

        #region input handlers

        private void HandleAttack()
        {
            if (!attackCooldown)
            {
                Debug.Log("attack pressed");
                didAttack = true;
                attackTypeCheck = StartCoroutine(AttackTypeCheck());
            }
        }

        private void HandleAttackCancel()
        {
            if (didAttack)
            {
                Debug.Log("attack released");
                didAttack = false;
                StartCoroutine(Attack());
            }
        }

        private void HandleScream()
        {

        }

        private void HandleScreamCancel()
        {

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
                DealDamage(realDamage);
                Debug.Log(message: $"did normal attack on enemy for {realDamage} damage");
            }
        }

        private void HeavyAttack()
        {
            canHeavyAttack = false;
            Debug.Log("heavy attack done");
            if (hitbox.canAttackEnemy && hitbox.targetEnemy != null)
            {
                DealDamage(realDamage * heavyAttackMult);
                Debug.Log(message: $"did heavy attack on enemy for {realDamage * heavyAttackMult} damage");
            }
        }

        private void DealDamage(float t_damage)
        {
            hitbox.targetEnemy.GetComponent<EnemyBase>().TakeDamage(t_damage);
            Debug.Log(message: $"did hit enemy for {t_damage} damage");
        }

        #endregion
    }
}