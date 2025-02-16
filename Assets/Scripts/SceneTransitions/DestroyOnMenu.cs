using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyOnMenu : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.activeSceneChanged += DestroyThisObject;
    }
    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= DestroyThisObject;
    }

    private void DestroyThisObject(Scene oldScene, Scene newScene)
    {
        if (newScene.name == "MainMenu")
        {
            Destroy(gameObject);
        }
    }
}
