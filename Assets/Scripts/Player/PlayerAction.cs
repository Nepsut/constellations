using UnityEngine;

namespace constellations
{
    public class PlayerAction : MonoBehaviour
    {
        #region variables

        [Header ("Engine Variables")]
        [SerializeField] private InputReader input;
        [SerializeField] private PolygonCollider2D polygonCollider;

        [Header("Constant Attack Variables")]
        private const int attackDamage = 20;
        private const int attackBuffAmount = 5;
        private const float attackSpeed = 1f;

        [Header("Dynamic Attack Variables")]
        private int attackBuffs = 0;            //add 1 every time player's attack gets buffed

        [Header("Interaction Variables")]
        private bool canInteractNPC = false;
        private bool canInteractObject = false;
        private GameObject interactingNPC;
        private GameObject interactingObject;


        #endregion

        #region standard methods

        private void Awake()
        {
            polygonCollider = GetComponentInChildren<PolygonCollider2D>();
            input.AttackEvent += HandleAttack;
            input.AttackCanceledEvent += HandleAttackCancel;
            input.ScreamEvent += HandleScream;
            input.ScreamCanceledEvent += HandleScreamCancel;
            input.InteractEvent += HandleInteract;
            input.InteractCanceledEvent += HandleInteractCancel;
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
                interactingNPC.transform.GetChild(0).gameObject.SetActive(false);
                interactingNPC = null;
            }
            else if (collision.gameObject.CompareTag("Interactable"))
            {
                canInteractObject = false;
                interactingObject = null;
            }
        }

        #endregion

        #region input handlers

        private void HandleAttack()
        {

        }

        private void HandleAttackCancel()
        {

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
        }

        private void HandleInteractCancel()
        {

        }

        #endregion
    }
}