using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public abstract class InteractBase : MonoBehaviour, IInteractable
    {
        private bool checkingInteractions = false;
        private GameObject player;

        private void Update()
        {
            if (checkingInteractions)
            {
                if (player.GetComponent<PlayerAction>().didInteractObject)
                {
                    Interact();
                    player.GetComponent<PlayerAction>().didInteractObject = false;
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
                    player = collider.gameObject;
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
                    player = null;
                }
            }
        }

        public abstract void Interact();
    }
}