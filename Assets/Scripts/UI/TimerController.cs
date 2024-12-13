using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour
{
    public static TimerController instance;

    [SerializeField] private TextMeshProUGUI timerCounter;

    private TimeSpan timePlaying;
    private bool timerGoing;

    private float elapsedTime;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        timerCounter.text = "GAME RUNNING TIME: 00:00.00";
        timerGoing = false;
    }

    //TimeController.instance.BeginTimer();
    public void BeginTimer()
    {
        timerGoing = true;
        elapsedTime = 0f;

        StartCoroutine(UpdateTimer());
    }

    public void EndTimer()
    {
        timerGoing = false;
    }

    private IEnumerator UpdateTimer()
    {
        while (timerGoing)
        {
            elapsedTime += Time.deltaTime;
            timePlaying = TimeSpan.FromSeconds(elapsedTime);
            string timePlayingTxt = "GAME RUNNING TIME: " + timePlaying.ToString("mm':'ss'.'ff");
            timerCounter.text = timePlayingTxt;

            yield return null;
        }
    }
}
