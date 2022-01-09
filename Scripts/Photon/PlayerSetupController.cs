using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;
using TMPro;

public class PlayerSetupController : GlobalEventListener
{
    [SerializeField] private Camera _sceneCamera;

    public GameObject teamSelector;
    public GameObject quitGameButton;

    public int TCount = 0;
    public TextMeshProUGUI TCountText = null;
    public int CTCount = 0;
    public TextMeshProUGUI CTCountText = null;
    public int roundLength = 0;
    public int roundAmount = 0;
    private Team _selectedTeam = Team.Blue;
    private System.Guid _eventID = System.Guid.Empty;

    public Button TButton;
    public Button CTButton;

    public Transform[] Red_Spawn_Points;
    public Transform[] Blue_Spawn_Points;

    public Camera SceneCamera { get => _sceneCamera; }

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        if (!BoltNetwork.IsServer)
            teamSelector.SetActive(true);
    }
    public override void SceneLoadRemoteDone(BoltConnection connection, IProtocolToken token)
    {
        if (BoltNetwork.IsServer)
        {
            UpdateTeamCount evnt = UpdateTeamCount.Create(connection, ReliabilityModes.ReliableOrdered);
            evnt.CounterTerrorist = CTCount;
            evnt.Terrorist = TCount;
            evnt.Send();
        }
    }
    //when a player chooses a team from the menu instantiate a prefab based on the selected team 
    public void SetTeam(int t)
    {
        CTButton.interactable = false;
        TButton.interactable = false;
        _selectedTeam = (Team)t;
        ChooseTeamEvent evnt = ChooseTeamEvent.Create(ReliabilityModes.ReliableOrdered);
        evnt.Team = t;
        _eventID = System.Guid.NewGuid();
        evnt.ID = _eventID;
        evnt.Send();

        SpawnPlayerEvent spawn = SpawnPlayerEvent.Create(GlobalTargets.OnlyServer);

        spawn.PlayerName = AppManager.Current.Username;
        spawn.PlayerTeam = (short)_selectedTeam;
        spawn.Send();

    }
    public override void OnEvent(ChooseTeamEvent evnt)
    {
        if (BoltNetwork.IsServer)
        {
            bool accepted = true;

            ConfirmTeamEvent @event = ConfirmTeamEvent.Create(ReliabilityModes.ReliableOrdered);
            @event.Team = evnt.Team;
            @event.IsAccepted = accepted;
            @event.ID = evnt.ID;

            @event.Send();

            if (accepted)
                AddTeamCount((Team)evnt.Team);
        }
    }
    //when a players chooses a team, add him to the  team count
    public void AddTeamCount(Team t)
    {
        if (t == Team.Blue)
            CTCount++;
        else
            TCount++;

        CTCountText.text = CTCount.ToString();
        TCountText.text = TCount.ToString();
    }
    //when a player leaves the server, call this function to remove him from the team count
    public void RemoveTeamCount(Team t)
    {
        if (t == Team.Blue)
            CTCount--;
        else
            TCount--;

        CTCountText.text = CTCount.ToString();
        TCountText.text = TCount.ToString();
    }
    public override void OnEvent(ConfirmTeamEvent evnt)
    {
        if (evnt.IsAccepted)
        {
            if (_eventID == evnt.ID)
            {
                teamSelector.SetActive(false);
                GUI_Controller.Current.SetTeam = (Team)evnt.Team;
            }
            if (BoltNetwork.IsClient)
                AddTeamCount((Team)evnt.Team);
        }
    }
    public override void OnEvent(SpawnPlayerEvent evnt)
    {
        var token = new PlayerTeamToken();
        token.playerTeam = (Team)evnt.PlayerTeam;
        token.playerName = evnt.PlayerName;
        BoltEntity entity;

        Vector3 v = Vector3.zero;

        if (token.playerTeam == Team.Blue)
        {
            v = GetSpawnPoint(token.playerTeam);

        }
        else if (token.playerTeam == Team.Red)
        {
            v = GetSpawnPoint(token.playerTeam);
        }

        switch ((Team)evnt.PlayerTeam)
        {
            case Team.Blue:
                entity = BoltNetwork.Instantiate(BoltPrefabs.Counter_Terrorist_Player, token, v, Quaternion.identity);
                entity.AssignControl(evnt.RaisedBy);
                break;
            case Team.Red:
                entity = BoltNetwork.Instantiate(BoltPrefabs.Terrorist_Player, token, v, Quaternion.identity);
                entity.AssignControl(evnt.RaisedBy);
                break;

        }
    }
    //Select a random spawn point from the map and spawn the player
    public Vector3 GetSpawnPoint(Team t)
    {
        Debug.Log("Function GetSpawnPoint is called");
        Vector3 v = Vector3.zero;

        if (t == Team.Blue)
        {
            v = Blue_Spawn_Points[Random.RandomRange(0, Blue_Spawn_Points.Length)].position;
            Debug.Log("GetSpawn point was called for team CT");

        }
        else if (t == Team.Red)
        {
            v = Red_Spawn_Points[Random.RandomRange(0, Red_Spawn_Points.Length)].position;
            Debug.Log("GetSpawn point was called for team T");
        }

        return v;
    }
}
