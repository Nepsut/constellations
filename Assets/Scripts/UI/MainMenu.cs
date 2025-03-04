using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuButtonHolder;

    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(2);
    }
    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit works");
    }

    public void ClearTutorialsSeen()
    {
        PlayerPrefs.SetInt("movementTutorial", 0);
        PlayerPrefs.SetInt("dashTutorial", 0);
        PlayerPrefs.SetInt("manaTutorial", 0);
        PlayerPrefs.SetInt("jumpTutorial", 0);
        PlayerPrefs.SetInt("combatTutorial", 0);
        PlayerPrefs.SetInt("crouchTutorial", 0);
        PlayerPrefs.SetInt("climbTutorial", 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SelectFirstChoice(menuButtonHolder));
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
