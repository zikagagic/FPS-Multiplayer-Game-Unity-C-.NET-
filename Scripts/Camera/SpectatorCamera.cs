using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

public class SpectatorCamera : MonoBehaviour
{

    private bool _forward = false;
    private bool _backward = false;
    private bool _left = false;
    private bool _right = false;
    private bool _elevate = false;
    private bool _drop = false;
    private float _speed = 5f;


    private float _yaw = 0;
    private float _pitch = 0;

    private Transform _target;


    private void PollKeys()
    {
        _forward = Input.GetKey(KeyCode.W);
        _backward = Input.GetKey(KeyCode.S);
        _left = Input.GetKey(KeyCode.A);
        _right = Input.GetKey(KeyCode.D);
        _elevate = Input.GetKey(KeyCode.Space);
        _drop = Input.GetKey(KeyCode.LeftControl);
        _yaw += Input.GetAxisRaw("Mouse X") * 2f;
        _yaw %= 360f;
        _pitch -= Input.GetAxisRaw("Mouse Y") * 2f;
    }
    
    void Update()
    {
        PollKeys();

        Vector3 movingDir = Vector3.zero;

        if(_forward ^ _backward)
        {
            movingDir += _forward ? transform.forward : -transform.forward;
        }

        if(_left ^ _right)
        {
            movingDir += _right ? transform.right : -transform.right;
        }

        if(_elevate ^ _drop)
        {
            movingDir += _elevate ? transform.up : -transform.up;
        }

        movingDir = Vector3.Normalize(movingDir);

        transform.position += movingDir * _speed * BoltNetwork.FrameDeltaTime;
        transform.rotation = Quaternion.Euler(transform.rotation.x + _pitch, transform.rotation.y + _yaw, 0f);
    }
}
