using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppManager : MonoBehaviour
{

    public HeadlessServerManager headlessServerManager = null;

    private static AppManager _current;
    public static AppManager Current
    {
        get
        {
            if (_current == null)
                _current = FindObjectOfType<AppManager>();
            return _current;
        }
    }

    public string Username
    {
        get
        {
            return PlayerPrefs.GetString("Username", "None");
        }

        set
        {
            PlayerPrefs.SetString("Username", value);
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    //If the game has been started as a client load the menu scene
    void Start()
    {
        if (!headlessServerManager.IsServer)
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
