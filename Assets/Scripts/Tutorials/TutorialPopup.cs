using System.Collections;
using TMPro;
using UnityEngine;

namespace constellations
{
    public class TutorialPopup : MonoBehaviour
    {
        [SerializeField] private string tutorialName = "";
        [SerializeField] private TextMeshProUGUI tutorialText;
        [SerializeField] private BoxCollider2D triggerBox;
        private const float destroyTime = 1.2f;

        private void Awake()
        {
            GetComponent<Canvas>().worldCamera = Camera.main;
        }

        private void Start()
        {
            if (PlayerPrefs.GetInt(tutorialName) == 1)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider == null) return;

            if (collider.gameObject.CompareTag("Player"))
            {
                PlayerPrefs.SetInt(tutorialName, 1);
                triggerBox.enabled = false;
                StartCoroutine(EaseDestroy());
            }
        }

        private IEnumerator EaseDestroy()
        {
            Color startColor = tutorialText.color;
            Color fadedColor = startColor;
            fadedColor.a = 0f;
            LeanTween.value(tutorialText.gameObject, UpdateColor, startColor, fadedColor, destroyTime).setEaseOutQuad();
            yield return new WaitForSeconds(destroyTime);
            Destroy(gameObject);
        }

        private void UpdateColor(Color _color)
        {
            tutorialText.color = _color;
        }
    }
}