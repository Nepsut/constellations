using constellations;
using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("Engine Variables")]
    [SerializeField] private InputReader input;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    public Story currentStory { get; private set; }

    [Header("Choices UI")]
    [SerializeField] private GameObject choiceHandler;
    [SerializeField] private GameObject choiceObject;
    private bool makingChoice = false;

    public static DialogueManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        input.SubmitEvent += HandleSubmit;
        input.ClickEvent += HandleClick;
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    public void EnterDialogue(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        dialoguePanel.SetActive(true);

        ContinueStory();
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            dialogueText.text = currentStory.Continue();
            DisplayChoices();
        }
        else
        {
            ExitDialogue();
        }
    }

    private void ExitDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        input.SetGameplay();
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;
        int index = 0;
        foreach (Choice choice in currentChoices)
        {
            int capturedIndex = index;
            GameObject t_choiceObject = Instantiate(choiceObject, choiceHandler.transform);
            t_choiceObject.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
            t_choiceObject.GetComponent<Button>().onClick.AddListener(() => MakeChoice(capturedIndex));
            Debug.Log(message: $"added choice listener with index {index}");
            index++;
        }
        if (index != 0)
        {
            StartCoroutine(SelectFirstChoice());
            makingChoice = true;
        }
    }

    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choiceHandler.transform.GetChild(0).gameObject);
    }
    public void MakeChoice(int choiceNumber)
    {
        Debug.Log(message: $"made choice number {choiceNumber}");
        currentStory.ChooseChoiceIndex(choiceNumber);
        makingChoice = false;

        foreach (Transform child in choiceHandler.transform)
        {
            Destroy(child.gameObject);
        }
        ContinueStory();
    }

    private void HandleSubmit()
    {
        if (!makingChoice) ContinueStory();
    }

    private void HandleClick()
    {
        if (!makingChoice) ContinueStory();
    }
}
