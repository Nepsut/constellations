using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuButtonHolder;

    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("2DUI");
    }
    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit works");

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
