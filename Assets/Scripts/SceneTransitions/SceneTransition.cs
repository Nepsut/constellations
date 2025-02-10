using UnityEngine;

namespace constellations
{
    public class SceneTransition : MonoBehaviour
    {
        [Header("Drag a ScriptableObject below")]
        [SerializeField] private SceneData levelToEnter;
        [Header("0 for Star Room, others in order!!!!")]
        [SerializeField] private int leavingLevel;
        private PlayerController player;

        private void Start()
        {
            if (player == null)
            player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider == null) return;

            if (collider.gameObject.CompareTag("Player"))
            {
                if (leavingLevel > 0)
                player.playedLevels[leavingLevel-1] = true;
                StartCoroutine(UIManager.instance.HandleLevelChange(levelToEnter.sceneID));
            }
        }
    }
}