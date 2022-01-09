using UnityEngine;
using Photon.Bolt;
using System;
using System.Collections;

public class GameController : EntityEventListener<ITestGameState>
{
    public GameObject MatchOver_SFX;
    private static GameController _gameInstance = null;
    private int _playerCountTarget = 1;
    public GamePhase _currentGamePhase = GamePhase.WaitForPlayers;

    //parameter to let the server know when it is tame to change a game phase
    public float _nextEvent = 0;

     
    private Team _matchWinner = Team.None;
    public  int winningRound=3;
    public  int roundLength=30;
    private bool endOfRound=false;

    private GameObject _localPlayer = null;
    public GameObject LocalPlayer
    {
       
        get => _localPlayer;
        set => _localPlayer = value;
    }

    public Team WinningTeam
    {
        get => _matchWinner;
        set => _matchWinner = value;
    }

    public static GameController Current
    {
        get
        {
            if (_gameInstance == null)
                _gameInstance = FindObjectOfType<GameController>();
            return _gameInstance;
        }
    }

    public GamePhase CurrentGamePhase { get => _currentGamePhase; }
    //set the game properties that are received when the server is being created
    public void Awake()
    {
        roundLength = HeadlessServerManager.GetRoundLength();
        winningRound = HeadlessServerManager.GetRoundAmount();
        _playerCountTarget = HeadlessServerManager.GetPlayerMinimum();
    }
    //assign the callbacks to state properties
    public override void Attached()
    {
        state.AddCallback("AlivePlayers", UpdatePlayersAlive);
        state.AddCallback("Blue_Points", UpdatePoints);
        state.AddCallback("Red_Points", UpdatePoints);
        state.AddCallback("Time", UpdateTime);
    }

    public void Update()
    {
        switch (_currentGamePhase)
        {
            case GamePhase.WaitForPlayers:
                break;
            case GamePhase.Starting:
                if(_nextEvent<BoltNetwork.ServerTime)
                {
                    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                    foreach(GameObject player in players)
                    {
                        player.GetComponent<PlayerCallbacks>().RoundReset();
                    }
                    _nextEvent = BoltNetwork.ServerTime + 5f;
                    state.Time = 5f;
                    _currentGamePhase = GamePhase.StartRound;
                    UpdateGameState();
                }
                break;
            case GamePhase.StartRound:
                if(_nextEvent<BoltNetwork.ServerTime)
                {
                    _nextEvent = BoltNetwork.ServerTime + roundLength;
                    state.Time = roundLength;
                    _currentGamePhase = GamePhase.Fighting;
                    UpdateGameState();
                }
                break;
            case GamePhase.Fighting:
                if (_nextEvent<BoltNetwork.ServerTime)
                {
                    CheckPlayerCount();
                    _nextEvent = BoltNetwork.ServerTime + 3f;
                    state.Time = 3f;
                    _currentGamePhase = GamePhase.EndRound;
                    UpdateGameState();
                }
                break;
            case GamePhase.EndRound:
                if(_nextEvent<BoltNetwork.ServerTime)
                {
                    _nextEvent = BoltNetwork.ServerTime + 5f;
                    state.Time = 5f;
                    _currentGamePhase = GamePhase.StartRound;
                    UpdateGameState();
                }
                break;
            case GamePhase.EndGame:
                break;
            default:
                break;
        }
    }
    //update the UI points
    public void UpdatePoints()
    {
        GUI_Controller.Current.UpdatePoints(state.Blue_Points,state.Red_Points);
        if(state.Red_Points==winningRound )
        {
            _currentGamePhase = GamePhase.EndGame;
            _matchWinner = Team.Red;
            UpdateGameState();
        }
        else if (state.Blue_Points == winningRound)
        {
            _currentGamePhase = GamePhase.EndGame;
            _matchWinner = Team.Blue;
            UpdateGameState();
        }
    }
    //Update the timer text
    public void UpdateTime()
    {
        GUI_Controller.Current.UpdateTimer(state.Time);
    }
    //Function that is called when a phase is changed
    public void UpdateGameState()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        switch(_currentGamePhase)
        {
            case GamePhase.WaitForPlayers:
                //if the current number of players are the minimum needed start the game
                if(_playerCountTarget==players.Length)
                {
                    _currentGamePhase = GamePhase.Starting;
                    _nextEvent = BoltNetwork.ServerTime + 3f;
                    state.Time = 3f;
                }
                break;
            case GamePhase.Starting:
                break;
            case GamePhase.StartRound:
                //reset the health of all players
                foreach(GameObject player in players)
                {
                    player.GetComponent<PlayerCallbacks>().RoundReset();
                }
                break;
            case GamePhase.Fighting:
                break;
            case GamePhase.EndRound:
                //kill all players the end of the round so that they can be respawned back to their base
                foreach (GameObject player in players)
                {
                    player.GetComponent<PlayerMotor>().state.IsDead = true;
                }
                break;
            case GamePhase.EndGame:
                //function that will end the game and declare the winner
                StartCoroutine(EndGameScreen());
                break;
            default:
                break;
        }
    }
   //check all players to count each teams alive members
    public void UpdatePlayersAlive()
    {
        int CT_Count = 0;
        int T_Count = 0;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if(entity.IsOwner)
        {
            foreach(GameObject player in players)
            {
                PlayerTeamToken pt = (PlayerTeamToken)player.GetComponent<PlayerMotor>().entity.AttachToken;
                if(!player.GetComponent<PlayerMotor>().state.IsDead)
                {
                    if (pt.playerTeam == Team.Blue)
                        CT_Count++;
                    else
                        T_Count++;
                }
            }
            //if all members of a team are killed assign a point to the other team
            if (_currentGamePhase==GamePhase.Fighting)
            {
                if(CT_Count==0)
                {
                    state.Red_Points++;
                    _nextEvent = BoltNetwork.ServerTime + 3f;
                    state.Time = 3f;
                    _currentGamePhase = GamePhase.EndRound;
                    UpdateGameState();
                }
                if(T_Count==0)
                {
                    state.Blue_Points++;
                    _nextEvent = BoltNetwork.ServerTime + 3f;
                    state.Time = 3f;
                    _currentGamePhase = GamePhase.EndRound;
                    UpdateGameState();
                }
            }

            if(GamePhase.WaitForPlayers == _currentGamePhase)
            {
                foreach(GameObject player in players)
                {
                   player.GetComponent<PlayerCallbacks>().RoundReset();
                }
            }
        }
    }
    //when a round ends and players are still alive check which team has more alive players to assign a point to 
    public void CheckPlayerCount()
    {
        int CT_Count = 0;
        int T_Count = 0;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (entity.IsOwner)
        {
            foreach (GameObject player in players)
            {
                PlayerTeamToken pt = (PlayerTeamToken)player.GetComponent<PlayerMotor>().entity.AttachToken;
                if (!player.GetComponent<PlayerMotor>().state.IsDead)
                {
                    if (pt.playerTeam == Team.Blue)
                        CT_Count++;
                    else
                        T_Count++;
                }
            }

            if (CT_Count != T_Count)
            {
                if (CT_Count > T_Count)
                {
                    state.Blue_Points++;
                }
                else
                {
                    state.Red_Points++;
                }
            }
        }
    }
    //reset both team's score and set the game phase to starting
    public void EndGameReset()
    {
        if(entity.IsOwner)
        {
            state.Blue_Points = 0;
            state.Red_Points = 0;
           _currentGamePhase = GamePhase.Starting;
        }
    }
    IEnumerator EndGameScreen()
    {
        MatchOver_SFX.GetComponent<AudioSource>().Play();
        GUI_Controller.Current.ShowEndGamePanel(true);
        GUI_Controller.Current._endGamePanel.TimeRemaining = 5f;
        yield return new WaitForSeconds(5f);
        GUI_Controller.Current.ShowEndGamePanel(false);
        EndGameReset();
    }
}
public enum GamePhase
{
    WaitForPlayers,
    Starting,
    StartRound,
    Fighting,
    EndRound,
    EndGame
}