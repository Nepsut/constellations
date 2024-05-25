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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision == null) return;
            if (collision.gameObject.CompareTag("NPC"))
            {
                canInteractNPC = true;
                interactingNPC = collision.gameObject;
                interactingNPC.transform.GetChild(0).gameObject.SetActive(true);
                Debug.Log(message: $"can interact with npc");
            }
            else if (collision.gameObject.CompareTag("Interactable"))
            {
                canInteractObject = true;
                interactingObject = collision.gameObject;
                Debug.Log(message: $"can interact with object");
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision == null) return;
            if (collision.gameObject.CompareTag("NPC"))
            {
                canInteractNPC = false;
                if (interactingNPC == null) return;
                interactingNPC.transform.GetChild(0).gameObject.SetActive(false);
                interactingNPC = null;
                Debug.Log(message: $"can't interact with npc");
            }
            else if (collision.gameObject.CompareTag("Interactable"))
            {
                canInteractObject = false;
                interactingObject = null;
                Debug.Log(message: $"can't interact with object");
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

        private void HandleInteract()
        {
            if (canInteractNPC && interactingNPC != null)
            {
                input.SetDialogue();
                interactingNPC.GetComponent<NPCDialogue>().Talk();
            }
        }

        private void HandleInteractCancel()
        {

        }

        #endregion
    }
}