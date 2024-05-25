using UnityEngine;

namespace constellations
{
    public class NPCDialogue : MonoBehaviour, IInteractable, ITalkable
    {
        [SerializeField] private TextAsset inkJSON;

        public void Interact()
        {

        }

        public void Talk()
        {
            DialogueManager.instance.EnterDialogue(inkJSON);
        }
    }
}