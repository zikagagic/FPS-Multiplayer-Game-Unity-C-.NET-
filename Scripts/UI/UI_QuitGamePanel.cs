using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Bolt;

public class UI_QuitGamePanel : MonoBehaviour
{
    public GameObject localPlayer;
    public void quitGame()
    {
        Application.Quit();
        
    }
}
