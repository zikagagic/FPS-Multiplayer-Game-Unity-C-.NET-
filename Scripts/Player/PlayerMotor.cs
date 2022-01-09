using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;


//Source/Template for script https://github.com/BoltEngine/Bolt-Sample/blob/master/AdvancedTutorial/scripts/Player/PlayerMotor.cs
public class PlayerMotor : EntityBehaviour<IPlayerState>
{
    //PlayerState parameters
    public struct State
    {
        public Vector3 position;
        public float rotation;
        public bool isGrounded;
        public bool isSprinting;
        public bool isWalking;
        public Vector3 velocity;
    }

    State stateMotor;

    [SerializeField]
    private Camera _cam = null;

    [SerializeField]
    private SphereCollider _headCollider=null;

    private CharacterController _cc = null;

    [SerializeField]private int _totalLife = 100;
    [SerializeField]private float _gravity = -80f;

    //parameters for speed and jump
    public float speedBase = 5f;
    public float sprintModifier = 1.25f;
    public float walkModifier = 0.5f;
    public float jumpHeight = 8f;
 
    [SerializeField] private bool  _isGrounded;
    [SerializeField] private bool _isSprinting = false;
    [SerializeField] private bool _isWalking = false;
    [SerializeField] private bool _isMoving = false;
    public AudioSource walkSFX;
    public AudioSource runSFX;
    public AudioSource sneakSFX;
    public AudioSource hitSFX;
    private bool _firstState = true;

    private bool _isEnemy = true;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private Vector3 _lastServerPos = Vector3.zero;

    [SerializeField]
    private LayerMask layerMask;

    public float Speed { get => speedBase; set => speedBase = value; }
    public int GetLife { get => _totalLife; }
    public bool IsEnemy { get => _isEnemy; }
    public bool IsSprinting { get => _isSprinting; }
    public bool IsWalking { get => _isWalking; }

    public bool IsMoving { get => _isMoving; }
   

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        stateMotor = new State();
    }
    

    public void Init(bool isMine)
    {
        if (isMine)
        {
            //chaning the tag of the player who is controlling the character to localplayer for easier differentiation
            tag = "LocalPlayer";
            GUI_Controller.Current.UpdateLife(_totalLife, _totalLife);
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            
            foreach (GameObject player in players)
            {
                Debug.Log(player.name.ToString());
                player.GetComponent<PlayerMotor>().TeamCheck();
                player.GetComponent<PlayerRenderer>().Init();
            }
        }
        TeamCheck();
        PlayerTeamToken lpt = (PlayerTeamToken)entity.AttachToken;
    }
    
    //
    public State ExecuteCommand(bool forward, bool backward, bool left, bool right, bool sprint, bool walk, bool jump, float yaw, float pitch)
    {
        if (!state.IsDead)
        {
            _isMoving = false;
            Vector3 movingDir = Vector3.zero;
           //player input
           if (forward ^ backward)
           {
             movingDir += forward ? transform.forward : -transform.forward;
           }
           if (left ^ right)
           {
             movingDir += right ? transform.right : -transform.right;
           }

            if ((movingDir.x != 0 || movingDir.z != 0) && !_isSprinting && !_isWalking)
            {
                _isMoving = true;     
            }
            _isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, layerMask);

            _isSprinting = sprint && _isGrounded && (forward || backward);

            _isWalking = walk && _isGrounded && (forward || backward);

            //if the player is moving based play a movement SFX
            if(_isMoving && _isGrounded)
            {
                if(!walkSFX.isPlaying)
                {
                    walkSFX.pitch = Random.Range(0.8f, 1);
                    walkSFX.volume = Random.Range(0.45f, 0.6f);
                    walkSFX.Play();
                }
            }

            if (_isSprinting && _isGrounded)
            {
                if (!runSFX.isPlaying)
                {
                    runSFX.pitch = Random.Range(0.8f, 1);
                    runSFX.volume = Random.Range(0.45f, 0.6f);
                    runSFX.Play();
                }
            }

            if (_isWalking && _isGrounded)
            {
                if (!sneakSFX.isPlaying)
                {
                    sneakSFX.pitch = Random.Range(0.8f, 1);
                    sneakSFX.volume = Random.Range(0.45f, 0.6f);
                    sneakSFX.Play();
                }
            }
            movingDir.Normalize();

            if (jump && _isGrounded)
            {
                movingDir.y = jumpHeight;
            }

            float currentSpeed = speedBase;

            if(_isSprinting)
            {
                currentSpeed *= sprintModifier;
            }
            else if(_isWalking)
            {
                currentSpeed *= walkModifier;
            }

            movingDir *= currentSpeed;
            //simulation of gravity that is constantly applied to the player
            movingDir.y += _gravity * 2F * Time.fixedDeltaTime;

            _cc.Move(movingDir * Time.fixedDeltaTime);

            _cam.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
            transform.rotation = Quaternion.Euler(0, yaw, 0);

            //Queing the pitch of the camera so that other players will see the weapon move and the direction it is shooting
            if (entity.IsOwner)
                //state.Pitch=(int)pitch;
                state.Pitch = pitch;

            stateMotor.isGrounded = _isGrounded;
            stateMotor.isSprinting = _isSprinting;
            stateMotor.isWalking = _isWalking;
            stateMotor.velocity = movingDir;
        }
        stateMotor.position = transform.position;
        stateMotor.rotation = yaw;

        return stateMotor;
    }

    public void SetPitch()
    {
        if (!entity.IsControllerOrOwner)
            _cam.transform.localEulerAngles = new Vector3(state.Pitch, 0f, 0f);
    }
    //Set the parameters of the state based on current values
    public void SetState(Vector3 position, float rotation, bool isGrounded, bool isSprinting, bool isWalking, Vector3 velocity)
    {
        if (Mathf.Abs(rotation - transform.rotation.y) > 5f)
            transform.rotation = Quaternion.Euler(0, rotation, 0);

        if (_firstState)
        {
            if (position != Vector3.zero)
            {
                transform.position = position;
                _firstState = false;
                _lastServerPos = Vector3.zero;
            }
        }
        else
        {
            if (position != Vector3.zero)
            {
                _lastServerPos = position;
            }

            transform.position += (_lastServerPos - transform.position) * 0.5f;
        }
        stateMotor.isGrounded = isGrounded;
        stateMotor.velocity = velocity;
        stateMotor.isSprinting = isSprinting;
        stateMotor.isWalking = isWalking;
    }

    void OnDrawGizmos()
    {
        //if the player is grounded show the gizmo as green for easier debuging
        if (Application.isPlaying)
        {
            Gizmos.color = stateMotor.isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
    //function that is called whenever the player receives damage
    public void Life(PlayerMotor shooter, int damage)
    {
        if(entity.IsOwner)
        {
            int value = state.LifePoints + damage;

            GUI_Controller.Current.ShowHitEffect();

            if (value<=0)
            {
                state.LifePoints = 0;
                state.IsDead = true;
            }
            else if(value>_totalLife)
            {
                state.LifePoints = _totalLife;
            }
            else
            {
                state.LifePoints = value;
            }
        }    
    }
    //Check all player's teams
    public void TeamCheck()
    {
        GameObject localPlayer = GameObject.FindGameObjectWithTag("LocalPlayer");
        Team localPlayerTeam = Team.Blue;
        PlayerTeamToken testToken = (PlayerTeamToken)entity.AttachToken;

        if(localPlayer)
        {
            PlayerTeamToken localPlayerToken = (PlayerTeamToken)localPlayer.GetComponent<PlayerMotor>().entity.AttachToken;
            localPlayerTeam = localPlayerToken.playerTeam;
        }
        if (testToken.playerTeam == localPlayerTeam)
            _isEnemy = false;
        else
            _isEnemy = true;
    }
    public void PlayHitSFX()
    {
        if (!hitSFX.isPlaying)
        {
            hitSFX.Play();
        }
    }
    public void OnDeath(bool isDead)
    {
        _cc.enabled = !isDead;
    }
}