using UnityEngine;

namespace constellations
{
    public class CrouchZone : MonoBehaviour
    {
        private PlayerController playerController;

        // Start is called before the first frame update
        void Start()
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        private void OnTriggerEnter2D(Collider2D collider2D)
        {
            if (collider2D == null) return;

            if (collider2D.gameObject.CompareTag("Player"))
            {
                playerController.EnterCrouchZone();
            }
        }

        private void OnTriggerExit2D(Collider2D collider2D)
        {
            if (collider2D == null) return;

            if (collider2D.gameObject.CompareTag("Player"))
            {
                playerController.ExitCrouchZone();
            }
        }
    }
}