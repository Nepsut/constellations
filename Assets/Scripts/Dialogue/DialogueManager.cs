using constellations;
using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    #region variables

    [Header("Engine Variables")]
    [SerializeField] private InputReader input;

    [Header("Dialogue UI")]
    [SerializeField] private string playerName;
    [SerializeField] private Sprite playerPortrait;
    private Sprite NPCPortrait;
    private string NPCName;
    [SerializeField] private Image dialogueImage;
    [SerializeField] private TextMeshProUGUI dialogueSpeaker;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    public Story currentStory { get; private set; }
    private bool isTyping = false;

    [Header("Choices UI")]
    [SerializeField] private GameObject choiceHandler;
    [SerializeField] private GameObject choiceObject;
    private const float timeBeforeChoices = 0.2f;
    private bool makingChoice = false;

    [Header("Save Variables")]
    private Vector3 saveSpot = Vector3.zero;

    [Header("Other Variables")]
    private const string alphaCode = "<color=#00000000>";
    private const float typeSpeed = 5f;
    private const float maxTypeTime = 0.1f;
    private bool stopTyping = false;
    private bool disableInput = false;
    public static DialogueManager instance { get; private set; }

    #endregion

    #region standard methods

    private void Awake()
    {
        if (instance == null) instance = this;
        if (playerName == null || playerName == "") playerName = "Car";
        input.SubmitEvent += HandleSubmit;
        input.ClickEvent += HandleClick;
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    #endregion

    #region dialogue methods

    //this enters dialogue with the inkJSON file assigned to the npc
    public void EnterDialogue(TextAsset inkJSON, Sprite t_portrait, string t_name, Vector3? position = null)
    {
        if (position == null) position = Vector3.zero;
        //first this sets the ink story as the active dialogue and activates dialogue panel
        currentStory = new Story(inkJSON.text);
        NPCPortrait = t_portrait;
        NPCName = t_name;
        saveSpot = (Vector3)position;
        dialoguePanel.SetActive(true);

        //continuestory prints dialogue so it's called here
        ContinueStory();
    }

    //dialogue printer
    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            StartCoroutine(TypeDialogue());
        }
        else
        { 
            //if no more story left, exit dialogue
            ExitDialogue();
        }
    }

    //this sets dialogue panel inactive, empties dialogue text and sets input scheme back to gameplay
    private void ExitDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        input.SetGameplay();
    }

    //choice printer
    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;
        int index = 0;
        //loop to instantiate a choice object onto the screen for every possible choice
        foreach (Choice choice in currentChoices)
        {
            int capturedIndex = index;      //can't use raw index as that will increase for each loop
            //instantiate object as child object of choice handler to let layoutgroup handle their positioning
            GameObject t_choiceObject = Instantiate(choiceObject, choiceHandler.transform);
            //instantiated object's text is set to the text of the current choice in list
            t_choiceObject.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
            //add listener to button so we can make choice based on... made choice
            t_choiceObject.GetComponent<Button>().onClick.AddListener(() => MakeChoice(capturedIndex));
            index++;
        }
        disableInput = false;
        //if there were choices, highlight choice and set makingChoice to true to disable inputs so story doesn't try to advance
        if (index != 0)
        {
            StartCoroutine(SelectFirstChoice());
            makingChoice = true;
        }
    }

    private IEnumerator SelectFirstChoice()
    {
        //unity apparently requires you to wait for the end of a frame until you can highlight an option so we do that
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choiceHandler.transform.GetChild(0).gameObject);
    }

    //this is called when choice is made to advance ink story based on made choice
    public void MakeChoice(int choiceNumber)
    {
        currentStory.ChooseChoiceIndex(choiceNumber);
        //choice is made, we are no longer making a choice and inputs are re-enabled
        makingChoice = false;

        //after choice is made, destroy active choice buttons
        foreach (Transform child in choiceHandler.transform)
        {
            Destroy(child.gameObject);
        }

        ContinueStory();
    }

    //this coroutine types the dialogue one letter at a time
    private IEnumerator TypeDialogue()
    {
        isTyping = true;
        dialogueText.text = "";
        string originalText = currentStory.Continue();
        string displayedText = "";
        int alphaIndex = 0;

        //grab tags from current line and show either player portrait or NPC portrait based on last tag in list
        if (currentStory.currentTags != null)
        {
            if (currentStory.currentTags.Last() == "Player")
            {
                dialogueSpeaker.text = playerName;
                dialogueImage.sprite = playerPortrait;
            }
            else
            {
                dialogueSpeaker.text = NPCName;
                dialogueImage.sprite = NPCPortrait;
            }
            if (currentStory.currentTags[0] == "save")
            {
                DataPersistenceManager.instance.SaveGame();
            }
        }

        foreach (char c in originalText.ToCharArray())
        {
            if (stopTyping) break;
            alphaIndex++;
            dialogueText.text = originalText;
            displayedText = dialogueText.text.Insert(alphaIndex, alphaCode);
            dialogueText.text = displayedText;
            yield return new WaitForSeconds(maxTypeTime / typeSpeed);
        }

        if (stopTyping) dialogueText.text = originalText;
        stopTyping = false;
        isTyping = false;
        //this is called on advance in case there are choices, does nothing if there are none
        disableInput = true;
        yield return new WaitForSeconds(timeBeforeChoices);
        DisplayChoices();
    }

    #endregion

    #region input handlers

    private void HandleSubmit()
    {
        if (!input.dialogueActive) return;
        if (isTyping)
        {
            stopTyping = true;
            return;
        }
        if (!makingChoice && !disableInput) ContinueStory();
    }

    private void HandleClick()
    {
        if (!input.dialogueActive) return;
        if (isTyping)
        {
            stopTyping = true;
            return;
        }
        if (!makingChoice && !disableInput) ContinueStory();
    }

    #endregion
}
