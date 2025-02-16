using System.Collections;
using UnityEngine;

namespace constellations
{
    public class SceneTransition : MonoBehaviour
    {
        [Header("Drag a ScriptableObject below")]
        [SerializeField] private SceneData levelToEnter;
        [Header("0 for Star Room, others in order!!!!")]
        [SerializeField] private int leavingLevel;
        [SerializeField] private bool levelGaveStar;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider == null) return;

            if (collider.gameObject.CompareTag("Player"))
            {
                UIManager.instance.StartLevelChange(levelToEnter, leavingLevel, levelGaveStar);
            }
        }
    }
}