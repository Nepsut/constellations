using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Cinemachine;

namespace constellations
{
    public class UIManager : MonoBehaviour
    {
        //singleton
        public static UIManager instance;

        [Header("Engine Variables")]
        [SerializeField] private InputReader input;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private CinemachineVirtualCamera cam;

        [Header("HUD")]
        [SerializeField] private Slider healthSlider;
        private const float healthLerpTime = 0.4f;
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
            if (instance != null)
            {
                Debug.Log("Found more than one MenuManager, fixing.");
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            LeanTween.init(1600);
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
            RefreshStars();
        }

        private void RefreshStars()
        {
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
            Debug.Log("healthchange called");
            float maxSliderValue = healthSlider.maxValue;
            float scaler = _maxHealth / maxSliderValue;
            float sliderValue = _currentHealth / scaler;
            LerpHealthChange(sliderValue);
        }

        private void LerpHealthChange(float _newHealth)
        {
            LeanTween.value(healthSlider.gameObject, SetHealthBar, healthSlider.value, _newHealth, healthLerpTime).setEaseOutQuad();
            StartCoroutine(TweenCancelDelay());
        }

        private IEnumerator TweenCancelDelay()
        {
            yield return new WaitForSeconds(healthLerpTime);
            healthSlider.gameObject.LeanCancel();
        }

        private void SetHealthBar(float _newHealth)
        {
            healthSlider.value = _newHealth;
        }

        public void ReloadLevel()
        {
            Time.timeScale = 1;
            //playerController.UnsubscribePlayerInputs();
            //DialogueManager.instance.UnsubscribeDialogueEvents();
            //input.PauseEvent -= HandlePause;
            playerController.ResetHealth();
            SceneManager.LoadScene(GameManager.instance.currentScene.sceneID);
            faintedScreen.SetActive(false);
            playerController.transform.position = GameManager.instance.currentScene.startPosition;
        }

        public void StartLevelChange(SceneData _sceneData, int _leavingLevel, bool _levelGaveStar)
        {
            StartCoroutine(HandleLevelChange(_sceneData, _leavingLevel, _levelGaveStar));
        }

        private IEnumerator HandleLevelChange(SceneData _sceneData, int _leavingLevel, bool _levelGaveStar)
        {
            TransitionFadeRect.gameObject.SetActive(true);
            playerController.invulnerableOverride = true;
            playerController.forceStationary = true;
            LeanTween.color(TransitionFadeRect, Color.black, transitionTime).setEaseInSine();
            yield return new WaitForSeconds(transitionTime);

            if (_levelGaveStar && !playerController.playedLevels[_leavingLevel-1])
            {
                playerController.playedLevels[_leavingLevel-1] = true;
                playerController.playerStars--;
            }
            RefreshStars();

            AsyncOperation asyncLoadScene = SceneManager.LoadSceneAsync(_sceneData.sceneID);
            while(!asyncLoadScene.isDone) yield return null;

            if (_sceneData.sceneID == 1) GameManager.instance.inStarRoom = true;
            else GameManager.instance.inStarRoom = false;
            GameManager.instance.currentScene = _sceneData;
            playerController.transform.position = _sceneData.startPosition;
            musicSource.clip = _sceneData.sceneMusic;
            musicSource.Play();
            LeanTween.color(TransitionFadeRect, Color.clear, transitionTime).setEaseOutSine();
            playerController.invulnerableOverride = false;
            playerController.forceStationary = false;
            if ((playerController.facingRight && !_sceneData.faceRightOnStart) || (!playerController.facingRight && _sceneData.faceRightOnStart))
            {
                playerController.CatFlip();
            }
            cam.m_Lens.OrthographicSize = _sceneData.lensOrtho;
            cam.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = _sceneData.screenY;

            yield return new WaitForSeconds(transitionTime);
            TransitionFadeRect.gameObject.SetActive(false);
        }

        public void QuitToMainMenu()
        {
            input.PauseEvent -= HandlePause;
            playerController.UnsubscribePlayerInputs();
            DialogueManager.instance.UnsubscribeDialogueEvents();
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