using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using System;
using UdpKit;
using TMPro;
using Photon.Bolt.Utils;
using UdpKit.Platform.Photon;

//Source/Template for code https://doc.photonengine.com/en-us/bolt/current/lobby-and-matchmaking/bolt-matchmaking
public class NetworkManager : GlobalEventListener
{
    [SerializeField] private TextMeshProUGUI feedback;
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField sessionName;
    private bool joinRandomRoom = false;
    private bool joinSpecificRoom = false;

    //Menu text that gives player feedabck while looking for a server
    public void FeedbackUser(string text)
    {
        feedback.text = text;
    }

    //Connect to a specific session ID
    public void Connect()
    {
        if (username.text != "" && sessionName.text != "")
        {
            joinSpecificRoom = true;
            AppManager.Current.Username = username.text;
            HeadlessServerManager.SetRoomID(sessionName.text);
            BoltLauncher.StartClient();

            FeedbackUser("Connecting  to server...");
        }
        else
            FeedbackUser("Enter a valid name");
    }

    //Connect to a random room
    public void ConnectToRandom ()
    {
        if(username.text != "")
        {
            joinRandomRoom = true;
            AppManager.Current.Username = username.text;
            BoltLauncher.StartClient();

            FeedbackUser("Searching for random server...");
        }
    }
    //Start searching for a session based on the players choice
    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        if (joinSpecificRoom)
        {
            JoinSession(HeadlessServerManager.GetRoomID());
        }
        else if(joinRandomRoom)
        {
            JoinRandomSession();
        }
    }
    public override void Connected(BoltConnection connection)
    {
        FeedbackUser("Connected !");
    }
    public override void Disconnected(BoltConnection connection)
    {
        FeedbackUser("Player disconnected: " + connection.RemoteEndPoint.ToString());
    }
    //Enter the server list menu
    public void StartServerList()
    {
        if (username.text != "")
        {
            AppManager.Current.Username = username.text;
            BoltLauncher.StartClient();
        }else
        {
            feedback.text = "Please enter a username!";
        }
    }
    //Shutdown the client that was instantiated so that there wont be an error when trying to enter a room or search again
    public void StopServerList()
    {
        BoltNetwork.Shutdown();
    }

    //Connect to a random available session (source https://doc.photonengine.com/en-us/bolt/current/lobby-and-matchmaking/bolt-matchmaking#join_by_session_name)
    public void JoinRandomSession()
    {
        if (BoltNetwork.IsRunning && BoltNetwork.IsClient)
        {
            BoltMatchmaking.JoinRandomSession();
        }
        else
        {
            FeedbackUser("Couldnt find an available server");
        }
    }
    //Connect to a specific session provided by the available session (source https://doc.photonengine.com/en-us/bolt/current/lobby-and-matchmaking/bolt-matchmaking#join_by_session_name)
    public void JoinSession(string sessionID)
    {
        if (BoltNetwork.IsRunning && BoltNetwork.IsClient)
        { 
            BoltMatchmaking.JoinSession(sessionID);
        }
        else
        {
            FeedbackUser("Session ID is invalid");
        }
    }
}
