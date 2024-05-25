using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace constellations
{
    public class NPCDialogue : MonoBehaviour, IInteractable, ITalkable
    {
        [SerializeField] private TextMeshPro hoverName;
        [SerializeField] private TextAsset inkJSON;
        [SerializeField] private Sprite portrait;
        [SerializeField] private string speakerName;

        private void Start()
        {
            hoverName.text = speakerName;
        }

        public void Interact()
        {
            //this is here because we inherit from IInteractable interface, which has the Interact() abstract method
            //...honestly forgot why this inheritance even exists here
        }

        //this is called by PlayerAction when player is in range of specific NPC
        //this then enters dialogue mode with specific ink story, handling dialogue and disabling other inputs
        public void Talk()
        {
            DialogueManager.instance.EnterDialogue(inkJSON, portrait, speakerName);
        }
    }
}