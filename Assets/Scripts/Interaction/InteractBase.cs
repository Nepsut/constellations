using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public abstract class InteractBase : MonoBehaviour, IInteractable
    {
        private bool checkingInteractions = false;
        private PlayerController playerController;

        private void Update()
        {
            if (checkingInteractions)
            {
                if (playerController.didInteractObject)
                {
                    Interact();
                    playerController.didInteractObject = false;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider != null )
            {
                if (collider.gameObject.CompareTag("Player"))
                {
                    checkingInteractions = true;
                    playerController = collider.gameObject.GetComponent<PlayerController>();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider != null)
            {
                if (collider.gameObject.CompareTag("Player"))
                {
                    checkingInteractions = false;
                    playerController = null;
                }
            }
        }

        public abstract void Interact();
    }
}