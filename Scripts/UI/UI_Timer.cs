using UnityEngine;
using UnityEngine.UI;
using TMPro;

//source https://gamedevbeginner.com/how-to-make-countdown-timer-in-unity-minutes-seconds/
public class UI_Timer : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public float timeRemaining;
    void Start()
    {
        timeText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    public void SetTimer(float timeToAdd)
    {
        timeRemaining = Time.time + timeToAdd;
    }

    void Update()
    {
        if(timeRemaining>Time.time)
        {
            timeText.text = ConvertFloatToTime(-(Time.time - timeRemaining));
        }    
    }

    public static string ConvertFloatToTime(float timeToConvert)
    {
        return string.Format("{0:#0}:{1:00}",
                    Mathf.Floor(timeToConvert / 60),//minutes
                    Mathf.Floor(timeToConvert) % 60);//seconds
    }
}
