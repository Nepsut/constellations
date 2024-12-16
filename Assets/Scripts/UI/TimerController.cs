using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour
{
    public static TimerController instance;

    [SerializeField] private TextMeshProUGUI timerCounter;

    public TimeSpan timePlaying { get; private set; }
    private bool timerGoing;

    private float elapsedTime;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Found more than one TimerController, fix this immediately!!");
        }
    }

    private void Start()
    {
        timerCounter.text = "GAME RUNNING TIME: 00:00.00";
        timerGoing = false;
        BeginTimer();
    }

    public void BeginTimer()
    {
        timerGoing = true;
        elapsedTime = 0f;

        StartCoroutine(UpdateTimer());
    }

    public void ToggleTimer(bool _pause)    //true pauses timer, false unpauses
    {
        if (_pause) timerGoing = false;
        else timerGoing = true;
    }

    private IEnumerator UpdateTimer()
    {
        while (true)
        {
            if (!timerGoing) yield return null;
            elapsedTime += Time.deltaTime;
            timePlaying = TimeSpan.FromSeconds(elapsedTime);
            string timePlayingTxt = "GAME RUNNING TIME: " + timePlaying.ToString("mm':'ss'.'ff");
            timerCounter.text = timePlayingTxt;

            yield return null;
        }
    }
}
