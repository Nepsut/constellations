using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace constellations
{
    public class UIManager : MonoBehaviour
    {
        //singleton
        public static UIManager instance;

        [Header("Engine Variables")]
        [SerializeField] private InputReader input;
        [SerializeField] private PlayerController playerController;

        [Header("HUD")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image chargeKnob;
        [SerializeField] private RectTransform heartHolder;
        private GameObject[] stars = new GameObject[9];

        [Header("Pause Menu")]
        [SerializeField] private GameObject pauseScreen;
        [SerializeField] private GameObject settingsScreen;
        [SerializeField] private GameObject menuHolder;
        [SerializeField] private GameObject settingsHolder;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button closeSettingsButton;

        [Header("Faint Menu")]
        [SerializeField] private GameObject faintedScreen;

        [Header("SceneManagement")]
        [SerializeField] private RectTransform TransitionFadeRect;
        [SerializeField] private Image TransitionFadeImg;
        private const float transitionTime = 2f;

        private bool gamePaused = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.LogError("Found more than one MenuManager, fix this!!");
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            //hide menus initially
            pauseScreen.SetActive(false);
            settingsScreen.SetActive(false);

            input.PauseEvent += HandlePause;

            //add methods to buttons without needing to make methods public
            resumeButton.onClick.AddListener(() => ResumeGame());
            settingsButton.onClick.AddListener(() => OpenSettings());
            closeSettingsButton.onClick.AddListener(() => CloseSettings());

            for (int i = 0; i < heartHolder.childCount; i++)
            {
                stars[i] = heartHolder.GetChild(i).gameObject;
                if (playerController.playerStars > i) continue;
                heartHolder.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void HandlePause()
        {
            //if not paused, pause, if paused, unpause
            if (!gamePaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }

        private void OpenSettings()
        {
            settingsScreen.SetActive(true);
            StartCoroutine(SelectFirstChoice(settingsHolder));
        }

        private void CloseSettings()
        {
            settingsScreen.SetActive(false);
            StartCoroutine(SelectFirstChoice(menuHolder));
        }

        public void ResumeGame()
        {
            input.SetGameplay();
            pauseScreen.SetActive(false);
            faintedScreen.SetActive(false);
            gamePaused = false;
            Time.timeScale = 1f;
        }

        private void PauseGame()
        {
            input.SetUI();
            pauseScreen.SetActive(true);
            gamePaused = true;
            Time.timeScale = 0f;
            StartCoroutine(SelectFirstChoice(menuHolder));
        }

        public void Fainted()
        {
            Time.timeScale = 0f;
            faintedScreen.SetActive(true);
        }

        //little knob above player to indicate heavy attack status
        public void ChargeUI(float _normalizedCharge)
        {
            chargeKnob.fillAmount = _normalizedCharge;
        }

        public void HealthUI(float _currentHealth, float _maxHealth)
        {
            float maxSliderValue = healthSlider.maxValue;
            float scaler = _maxHealth / maxSliderValue;
            float sliderValue = _currentHealth / scaler;
            healthSlider.value = sliderValue;
        }

        public void ReloadLevel()
        {
            Time.timeScale = 1;
            playerController.UnsubscribePlayerInputs();
            DialogueManager.instance.UnsubscribeDialogueEvents();
            input.PauseEvent -= HandlePause;
            SceneManager.LoadScene(1);
        }

        public IEnumerator HandleLevelChange(int _levelToLoad)
        {
            TransitionFadeRect.gameObject.SetActive(true);
            playerController.invulnerableOverride = true;
            playerController.forceStationary = true;
            LeanTween.color(TransitionFadeRect, Color.black, transitionTime).setEaseInSine();
            yield return new WaitForSeconds(transitionTime);

            AsyncOperation asyncLoadScene = SceneManager.LoadSceneAsync(_levelToLoad);
            while(!asyncLoadScene.isDone) yield return null;

            LeanTween.color(TransitionFadeRect, Color.clear, transitionTime).setEaseOutSine();
            yield return new WaitForSeconds(transitionTime);
            playerController.invulnerableOverride = false;
            playerController.forceStationary = false;
            TransitionFadeRect.gameObject.SetActive(false);
        }

        public void QuitToMainMenu()
        {
            input.PauseEvent -= HandlePause;
            playerController.UnsubscribePlayerInputs();
            SceneManager.LoadScene("MainMenu");
        }

        //highlight first option from list
        private IEnumerator SelectFirstChoice(GameObject menuList)
        {
            //unity apparently requires you to wait for the end of a frame until you can highlight an option so we do that
            EventSystem.current.SetSelectedGameObject(null);
            yield return new WaitForEndOfFrame();
            if (menuList.transform.GetChild(0).gameObject != null)
            {
                EventSystem.current.SetSelectedGameObject(menuList.transform.GetChild(0).gameObject);
            }
        }
    }
}