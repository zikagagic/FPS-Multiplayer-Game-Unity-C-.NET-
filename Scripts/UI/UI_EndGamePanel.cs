using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_EndGamePanel : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI winningTeamText;
    [SerializeField] private TextMeshProUGUI serverCountdownText;
    private float _timeRemaining = 5f;

    public float TimeRemaining
    {
        set => _timeRemaining = value;
    }
    //Update the text based on which team won
    void Update()
    {
        calculateTime();
        winningTeamText.text = "Winning Team:" + GameController.Current.WinningTeam;
    }

    //function that will start a countdown for the server to be reset SOURCE(https://gamedevbeginner.com/how-to-make-countdown-timer-in-unity-minutes-seconds/)
    public void calculateTime()
    {
        if(_timeRemaining > 0 )
        {
            _timeRemaining -= Time.deltaTime;
            serverCountdownText.text = string.Format("Restarting server in: {0:#0}", _timeRemaining);
        }
        else
        {
            _timeRemaining = 0;
        }
       
    }
}
