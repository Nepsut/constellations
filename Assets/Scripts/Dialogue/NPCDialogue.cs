using TMPro;
using UnityEngine;

namespace constellations
{
    public class NPCDialogue : MonoBehaviour, ITalkable
    {
        [SerializeField] private TextMeshPro hoverName;
        [SerializeField] private TextAsset inkJSON;
        [SerializeField] private Sprite portrait;
        [SerializeField] private AudioClip npcVoice;
        [SerializeField] private string speakerName;
        [SerializeField] Canvas canvasObject;

        private void Awake()
        {
            canvasObject.worldCamera = Camera.main;
        }

        private void Start()
        {
            hoverName.text = speakerName;
        }

        //this is called by PlayerAction when player is in range of specific NPC
        //this then enters dialogue mode with specific ink story, handling dialogue and disabling other inputs
        public void Talk()
        {
            DialogueManager.instance.EnterDialogue(inkJSON, npcVoice, speakerName, DoAfterDialogue, portrait);
        }

        protected virtual void DoAfterDialogue() { }
    }
}