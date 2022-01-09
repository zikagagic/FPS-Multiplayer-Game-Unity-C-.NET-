using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Bolt;
using Photon.Bolt.Utils;
using Photon.Bolt.Matchmaking;
using System;

public class HeadlessServerManager : GlobalEventListener
{
    //Source/Template for code https://doc.photonengine.com/en-us/bolt/current/demos-and-tutorials/headless-server
    [SerializeField]
    private string _map = "";
    private static string s_map;

    [SerializeField]
    private bool _hasSound = true;
    private static bool s_hasSound=true;

    [SerializeField]
    private string _roomID = "Test";
    private static string s_roomID;

    [SerializeField]
    private string _roundAmount;
    private static string s_roundAmount;
    [SerializeField]
    private string _roundLength;
    private static string s_roundLength;
    [SerializeField]
    private string _playerMinimum;
    private static string s_playerMinimum;

    private static int roundLengthInt;
    private static int roundAmountInt;
    private static int playerMinimum;

    [SerializeField]
    private bool _isServer = false;

    public bool IsServer { get => _isServer; set => _isServer = value; }

    public static string GetRoomID()
    {
        return s_roomID;
    }

    public static int GetRoundLength()
    {
        return roundLengthInt;
    }
    
    public static int GetRoundAmount()
    {
        return roundAmountInt;
    }
    public static int GetPlayerMinimum()
    {
        return playerMinimum;
    }
    public static string SetRoomID(string RoomID)
    {
        s_roomID = RoomID;

        return s_roomID;
    }

    public static string Map()
    {
        return s_map;
    }

    //register the room properties
    public override void BoltStartBegin()
    {
        BoltNetwork.RegisterTokenClass<PhotonRoomProperties>();
    }

    public override void BoltStartDone()
    {
        if (BoltNetwork.IsServer)
        {
            //if the game has been instantiated as a server, mute the audio in game
            AudioListener.pause = true;
            PhotonRoomProperties roomProperties = new PhotonRoomProperties();

            roomProperties.AddRoomProperty("m", _map);
            roomProperties.AddRoomProperty("rLength", _roundLength);
            roomProperties.AddRoomProperty("rAmmount", _roundAmount);
            roomProperties.AddRoomProperty("minPlayers", _playerMinimum);

            roomProperties.IsOpen = true;
            roomProperties.IsVisible = true;

            //If RoomID is not set, generate a random one
            if (s_roomID.Length == 0)
            {
                s_roomID = Guid.NewGuid().ToString();
            }
            //Create a server based on the given parameters
            BoltMatchmaking.CreateSession(
                sessionID: s_roomID,
                token: roomProperties,
                sceneToLoad: _map
            );
        }
    }

    private void Awake()
    {
        //namestanje svih vrednosti servera
        _isServer = "true" == (GetArg("-s", "-isServer") ?? (_isServer ? "true" : "false"));
        //mapa koja ce biti izabrana za igru
        s_map = GetArg("-m", "-map") ?? _map;
        //naziv servera koji ce mu biti dodeljen
        s_roomID = GetArg("-r", "-room") ?? _roomID;
        //vremensko trajanje pojedinacne runde
        s_roundLength=GetArg("-rLength","-roundlength") ?? _roundLength;
        //koliko rundi je potrebno za pobedu
        s_roundAmount = GetArg("-rAmount", "-roundamount") ?? _roundAmount;
        //minimalan broj igraca potreban za pokretanje igre
        s_playerMinimum = GetArg("-minPlayers", "-playerMinimum") ?? _playerMinimum;

        _hasSound = "true" == (GetArg("-sound", "-hasSound") ?? (_hasSound ? "true" : "false"));

        roundLengthInt = Int32.Parse(s_roundLength);
        roundAmountInt = Int32.Parse(s_roundAmount);
        playerMinimum = Int32.Parse(s_playerMinimum);

        if (IsServer)
        {
            //ako je igra pokrenuta kao server, pauziraj zvuk u igri
            AudioListener.pause = true;

            var validMap = false;

            //proveri sve scene u projektu, ako ime scene odgovara zadatoj vrednosti dodeli tu scenu kao mapu za igru
            foreach (string value in BoltScenes.AllScenes)
            {
                if (SceneManager.GetActiveScene().name != value)
                {
                    if (s_map == value)
                    {
                        validMap = true;
                        break;
                    }
                }
            }

            if (!validMap)
            {
                BoltLog.Error("Invalid configuration: please verify level name");
                Application.Quit();
            }
            //igra se pokrece kao server sa svim parametrima koja je uzela
            BoltLauncher.StartServer();
            DontDestroyOnLoad(this);
        }
    }
    //proveri argument, ako argument odgovara zadatom imenu, uzima se vrednost pored parametra
    static string GetArg(params string[] names)
    {
        //uzimaju se sve vrednosti koje su unete preko komandne linije
        var args = Environment.GetCommandLineArgs();
        //prolazak kroz sve vrednosti koje su unete radi provere validnosti
        for (int i = 0; i < args.Length; i++)
        {
            foreach (var name in names)
            {   //ukoliko vrednost argumenta odgovara nekom od postojecih parametera za server, dodeli vrednost tom parametru
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
        }
        return null;
    }
}
