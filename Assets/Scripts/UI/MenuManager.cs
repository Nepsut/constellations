using System.Collections;
using System.Collections.Generic;
using constellations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class MenuManager : MonoBehaviour
{
    [Header("Engine Variables")]
    [SerializeField] private InputReader input;
    [SerializeField] private GameObject menuScreen;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject menuHolder;
    [SerializeField] private GameObject settingsHolder;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button closeSettingsButton;


    private bool gamePaused = false;
    // Start is called before the first frame update
    void Start()
    {
        //hide menus initially
        menuScreen.SetActive(false);
        settingsScreen.SetActive(false);

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

    private void ResumeGame()
    {
            input.SetGameplay();
            menuScreen.SetActive(false);
            gamePaused = false;
            Time.timeScale = 1f;
    }

    private void PauseGame()
    {
            input.SetUI();
            menuScreen.SetActive(true);
            gamePaused = true;
            Time.timeScale = 0f;
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