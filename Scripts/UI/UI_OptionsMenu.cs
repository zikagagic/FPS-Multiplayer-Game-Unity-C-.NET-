using Photon.Bolt;
using System.Collections;
using System.Collections.Generic;
using UdpKit.Platform.Photon;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class UI_OptionsMenu : MonoBehaviour
{
    //sourec for code https://www.youtube.com/watch?v=YOaYQrN1oYQ

    public GameObject onCheckmark;
    public List<PhotonSession> sessionList = new List<PhotonSession>();
    Resolution[] resolutions;
    public TMPro.TMP_Dropdown resolutionDropwdown;
    private void Start()
    {
        //get current resolution
        resolutions = Screen.resolutions;
        resolutionDropwdown.ClearOptions();
        List<string> options = new List<string>();

        int currentResolutionIndex;

        //every resoultion that is available for the current player's monitor will be gathered and added into the list
        for (int i=0;i<resolutions.Length;i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if(resolutions[i].width ==Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropwdown.AddOptions(options);
    }
    public AudioMixer mainMixer;
    //set the game volume according to the field value of the optins menu slider
    public void SetVolume(float volume)
    {
        Debug.Log(volume);
        mainMixer.SetFloat("Volume", volume);
    }

    //set the game graphcs according to the selected graphics quality from the menu
    public void SetGraphics(int indexOfElement)
    {
        QualitySettings.SetQualityLevel(indexOfElement);
        Debug.Log(indexOfElement);
    }
    //toggle fullscreen 
    public void SetFullScreen(bool isToggled)
    {
        Screen.fullScreen = isToggled;
    }
    //set the game resolution

    public void SetResolution (int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public void FindServer()
    {
        foreach (var session in BoltNetwork.SessionList)
        {
            var photonSession = session.Value as PhotonSession;
            sessionList.Add(photonSession);
        }
    }
}
