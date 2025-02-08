using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace constellations
{
    public abstract class InteractBase : MonoBehaviour, IInteractable
    {
        private bool checkingInteractions = false;
        [SerializeField] protected PlayerController playerController;

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
            if (collider == null) return;
            if (collider.gameObject.CompareTag("Player"))
            {
                checkingInteractions = true;
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            if (collider == null) return;
            if (collider.gameObject.CompareTag("Player"))
            {
                checkingInteractions = false;
            }
        }

        public abstract void Interact();
    }
}