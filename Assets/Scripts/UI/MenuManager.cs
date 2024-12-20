using System.Collections;
using System.Collections.Generic;
using constellations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    //singleton
    public static MenuManager instance;

    [Header("Engine Variables")]
    [SerializeField] private InputReader input;
    [SerializeField] private GameObject miniMap;

    [Header("HUD")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image chargeKnob;
    [SerializeField] private TextMeshProUGUI scoreTextGame;
    [SerializeField] private TextMeshProUGUI killCountTextGame;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject menuHolder;
    [SerializeField] private GameObject settingsHolder;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private TextMeshProUGUI pauseScore;

    [Header("Faint Menu")]
    [SerializeField] private GameObject faintedScreen; 
    [SerializeField] private TextMeshProUGUI runtimeFainted;
    [SerializeField] private TextMeshProUGUI killcountFainted;
    [SerializeField] private TextMeshProUGUI scoreFainted;
    [SerializeField] private TextMeshProUGUI slashCountFainted;

    [Header("Killfeed Items")]
    [SerializeField] private Transform killfeedHolder;
    [SerializeField] private GameObject killTextObject;
    private const float killfeedPersistTime = 4f;
    private const float killfeedFadeTime = 2f;
    private int killCount = 0;
    private int score = 0;


    //player
    [SerializeField] private PlayerController playerController;

    private bool gamePaused = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Found more than one MenuManaged, fix this!!");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //hide menus initially
        pauseScreen.SetActive(false);
        settingsScreen.SetActive(false);
        miniMap.SetActive(true);

        input.PauseEvent += HandlePause;

        //add methods to buttons without needing to make methods public
        resumeButton.onClick.AddListener(() => ResumeGame());
        settingsButton.onClick.AddListener(() => OpenSettings());
        closeSettingsButton.onClick.AddListener(() => CloseSettings());
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
        TimerController.instance.ToggleTimer(true);    //true to unpause timer
        Time.timeScale = 1f;
    }

    private void PauseGame()
    {
        input.SetUI();
        pauseScreen.SetActive(true);
        gamePaused = true;
        pauseScore.text = string.Concat("SCORE: ", score);
        TimerController.instance.ToggleTimer(false);    //false to pause timer
        Time.timeScale = 0f;
        StartCoroutine(SelectFirstChoice(menuHolder));
    }

    public void Fainted()
    {
        Time.timeScale = 0f;
        faintedScreen.SetActive(true);
        miniMap.SetActive(false);

        runtimeFainted.text = string.Concat("GAME RUNNING TIME: ", TimerController.instance.timePlaying.ToString("mm':'ss'.'ff"));
        killcountFainted.text = string.Concat("ELIMINATED ENEMIES: ", killCount);
        scoreFainted.text = string.Concat("SCORE: ", score);
        slashCountFainted.text = string.Concat("SLASHED: ", playerController.totalSlashAttacks);
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

    public void EnemyDied(string _killerName, string _victimName, int _increaseScoreBy)
    {
        TextMeshProUGUI killText = Instantiate(killTextObject, killfeedHolder).GetComponent<TextMeshProUGUI>();
        killText.text = string.Concat("-[", _killerName, "]", " killed a ", _victimName);
        score += _increaseScoreBy;
        killCount++;
        UpdateScores();
        StartCoroutine(FadeKillfeedItem(killText));
    }

    public void GotMana(float _manaAmount)
    {
        TextMeshProUGUI manaText = Instantiate(killTextObject, killfeedHolder).GetComponent<TextMeshProUGUI>();
        manaText.text = string.Concat("-[Player]", " recovered ", _manaAmount, " mana");
        StartCoroutine(FadeKillfeedItem(manaText));
    }

    private void UpdateScores()
    {
        killCountTextGame.text = killCount.ToString();
        scoreTextGame.text = string.Concat("SCORE: ", score);
    }

    private IEnumerator FadeKillfeedItem(TextMeshProUGUI _killText)
    {
        float timer = 0;
        Color fadeColor = _killText.color;
        float startingAlpha = fadeColor.a;

        yield return new WaitForSeconds(killfeedPersistTime);

        while (timer < killfeedFadeTime)
        {
            timer += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(startingAlpha, 0, timer / killfeedFadeTime);
            _killText.color = fadeColor;
            yield return null;
        }
        Destroy(_killText.gameObject);
    }

    public void ReloadLevel()
    {
        Time.timeScale = 1;
        playerController.UnsubscribePlayerInputs();
        input.PauseEvent -= HandlePause;
        SceneManager.LoadScene(1);
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