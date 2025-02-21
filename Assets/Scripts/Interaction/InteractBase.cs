using UnityEngine;

namespace constellations
{
    public abstract class InteractBase : MonoBehaviour, IInteractable
    {
        protected PlayerController playerController;

        private void Start()
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        public abstract void Interact();
    }
}