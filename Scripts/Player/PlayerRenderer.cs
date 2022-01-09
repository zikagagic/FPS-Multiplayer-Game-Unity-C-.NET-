using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

public class PlayerRenderer : EntityBehaviour<IPlayerState>
{
    private PlayerMotor _playerMotor;
    [SerializeField] private GameObject renderer;
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _sceneCamera;
    [SerializeField] private TMPro.TextMeshPro _playerName;
    

    private void Awake()
    {
        _playerMotor = GetComponent<PlayerMotor>();
    }
    public void Init()
    {
        if (entity.IsControllerOrOwner)
            _camera.gameObject.SetActive(true);
        if (entity.HasControl)
        {
            _sceneCamera = FindObjectOfType<PlayerSetupController>().SceneCamera.transform;
            _sceneCamera.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            PlayerTeamToken playerTeamToken = (PlayerTeamToken)entity.AttachToken;
            _playerName.text = playerTeamToken.playerName;
            renderer.SetActive(true);
        }
    }
    public void OnDeath(bool IsDead)
    {
        if(IsDead)
        {
            if (entity.HasControl)
                _sceneCamera.gameObject.SetActive(true);
            _camera.gameObject.SetActive(false);
            renderer.SetActive(false);
            _playerName.gameObject.SetActive(false);

        }
        else
        {
            if (entity.IsControllerOrOwner)
                _camera.gameObject.SetActive(true);
            if(entity.HasControl)
            {
                _sceneCamera.gameObject.SetActive(false);
            }
            else
            {
                renderer.SetActive(true);
                _playerName.gameObject.SetActive(true);

            }
        }
    }
}
