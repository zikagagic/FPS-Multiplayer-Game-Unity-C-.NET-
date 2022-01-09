using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

//source/template for code https://github.com/BoltEngine/Bolt-Sample/blob/master/AdvancedTutorial/scripts/Player/PlayerController.cs
public class PlayerController : EntityBehaviour<IPlayerState>
{
    public PlayerMotor _playerMotor;
    private PlayerWeapons _playerWeapons;
    private PlayerRenderer _playerRenderer;
    //input paramaters for moving
    private bool _forward;
    private bool _backward;
    private bool _left;
    private bool _right;
    private bool _jump;
    private float _yaw;
    private float _pitch;
    private bool _sprint;
    private bool _walk;

    //input parameters for shooting
    private bool _fire;
    private bool _fireOnceDown;
    private bool _fireOnceUp;
    private bool _aiming;
    private bool _reload;

    //input parameters for changing weapons;
    private int _wheel;
    private bool _selectPrimaryWeapon;
    private bool _selectSecondaryWeapon;
    //check if player is in a menu
    public bool _isInMenu = false;
    public bool _isOwner = false;
    public bool _hasControl = false;

    private float _mouseSensitivity = 6f;

    public float MouseSensitivity { get => _mouseSensitivity; set => _mouseSensitivity = value; }
    public int Wheel { get => _wheel; set => _wheel = value; }

    public void Awake()
    {
        _playerMotor = GetComponent<PlayerMotor>();
        _playerWeapons = GetComponent<PlayerWeapons>();
        _playerRenderer = GetComponent<PlayerRenderer>();
    }
    //when a player attaches to the server
    public override void Attached()
    {
        state.SetTransforms(state.Transform, transform);
        if (entity.HasControl)
        {      
            _hasControl = true;
            GameController.Current.LocalPlayer = gameObject;
        }
        Init(_hasControl);
        _playerMotor.Init(_hasControl);
        _playerRenderer.Init();
        _playerWeapons.Init();
       
    }
    //when a player gains control of a prefab show the GUI
    public override void ControlGained()
    {
       GUI_Controller.Current.Show_GUI(true);
    }
    public void Init(bool isMine)
    {
        if (isMine)
        {
            FindObjectOfType<PlayerSetupController>().SceneCamera.gameObject.SetActive(false);
        }
    }
    public override void Detached()
    {
        base.Detached();
    }
    private void Update()
    {
        if (_hasControl)
        {
            //When the player presses M select the weapon menu
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (_isInMenu)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    PauseKeys();
                }
                _isInMenu ^= true;
                GUI_Controller.Current.ShowWeaponPanel(_isInMenu);
            }
            //When the player preses ESC show the quit game panel
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if (_isInMenu)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    PauseKeys();
                }
                _isInMenu ^= true;
                GUI_Controller.Current.ShowQuitGamePanel(_isInMenu);

            }
            if(!_isInMenu)
                PollKeys();
        }
           
    }
    //Poll all the keys the player has pressed and send them over the network
    private void PollKeys()
    {
        _forward = Input.GetKey(KeyCode.W);
        _backward = Input.GetKey(KeyCode.S);
        _left = Input.GetKey(KeyCode.A);
        _right = Input.GetKey(KeyCode.D);
        _jump = Input.GetKey(KeyCode.Space);
        _sprint = Input.GetKey(KeyCode.LeftShift);
        _walk = Input.GetKey(KeyCode.LeftControl);

        _fire = Input.GetMouseButton(0);
        _fireOnceDown = Input.GetMouseButtonDown(0);
        _fireOnceUp = Input.GetMouseButtonUp(0);
        _aiming = Input.GetMouseButton(1);
        _reload = Input.GetKey(KeyCode.R);

        _yaw += Input.GetAxisRaw("Mouse X") * _mouseSensitivity;
        _yaw %= 360f;
        _pitch += -Input.GetAxisRaw("Mouse Y") * _mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, -75, 75);

        _selectPrimaryWeapon = Input.GetKey(KeyCode.Alpha1);
        _selectSecondaryWeapon = Input.GetKey(KeyCode.Alpha2);
    }
    //if the player is in a menu pause the polling of keys
    private void PauseKeys()
    {
        _forward = false;
        _backward = false;
        _left = false;
        _right = false;
        _jump = false;

        _fire = false;
        _aiming = false;
        _reload = false;
    }

    //Only runs on the person which has been assigned control of an entity
    public override void SimulateController()
    {
        IPlayerCommandInput input = PlayerCommand.Create();
        input.forward = _forward;
        input.backward = _backward;
        input.right = _right;
        input.left = _left;

        input.fire = _fire;
        input.aim = _aiming;
        input.reload = _reload;
        input.fireOnceDown = _fireOnceDown;
        input.fireOnceUp = _fireOnceUp;

        input.yaw = _yaw;
        input.pitch = _pitch;
        input.jump = _jump;
        input.sprint = _sprint;
        input.walk = _walk;
        input.wheel = _wheel;
        input.selectPrimaryWeapon = _selectPrimaryWeapon;
        input.selectSecondaryWeapon = _selectSecondaryWeapon;

        //Que all the input and send it to the other players and the server
        entity.QueueInput(input);

        _playerMotor.ExecuteCommand(_forward, _backward, _left, _right,_sprint,_walk, _jump, _yaw, _pitch);
        _playerWeapons.ExecuteCommand(_fire,_fireOnceDown,_fireOnceUp, _aiming,_wheel, _reload,_selectPrimaryWeapon,_selectSecondaryWeapon);
    }

    //Run all the inputs to the player motor and player weapons script
    public override void ExecuteCommand(Command command, bool resetState)
    {
        PlayerCommand cmd = (PlayerCommand)command;

        if (resetState)
        {
            _playerMotor.SetState(cmd.Result.Position, cmd.Result.Rotation, cmd.Result.isGrounded,cmd.Result.isSprinting,cmd.Result.isWalking, cmd.Result.Velocity);
        }
        else
        {
            PlayerMotor.State motorState = new PlayerMotor.State();
            if (!entity.HasControl)
            {
                motorState = _playerMotor.ExecuteCommand(
                    cmd.Input.forward,
                    cmd.Input.backward,
                    cmd.Input.left,
                    cmd.Input.right,
                    cmd.Input.sprint,
                    cmd.Input.walk,
                    cmd.Input.jump,
                    cmd.Input.yaw,
                    cmd.Input.pitch);

                _playerWeapons.ExecuteCommand(
                    cmd.Input.fire,
                    cmd.Input.fireOnceDown,
                    cmd.Input.fireOnceUp,
                    cmd.Input.aim,
                    cmd.Input.wheel,
                    cmd.Input.reload,
                    cmd.Input.selectPrimaryWeapon,
                    cmd.Input.selectSecondaryWeapon);
            }
            cmd.Result.Position = motorState.position;
            cmd.Result.Rotation = motorState.rotation;
            cmd.Result.isGrounded = motorState.isGrounded;
            cmd.Result.isWalking = motorState.isWalking;
            cmd.Result.isSprinting = motorState.isSprinting;
        }
    }
}
