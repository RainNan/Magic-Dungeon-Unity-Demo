using System;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    public Transform orientation;
    public Transform playerObj;
    
    private Player _player;
    public float rotationSpeed = 10f;

    private Transform mainCam;

    private void Awake()
    {
        _player = GetComponentInParent<Player>();
    }

    private void Start()
    {
        mainCam = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        
        Cursor.visible = false;
    }

    private void Update()
    {
        if (mainCam == null) return;

        if (!_player.CanRotation) return;

        // 用真正主相机的 forward 来更新 orientation
        Vector3 camForward = mainCam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        orientation.forward = camForward;

        // 根据输入决定角色朝向
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;
        inputDir.y = 0f;

        if (inputDir.sqrMagnitude > 0.001f)
        {
            Vector3 targetDir = inputDir.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDir, Vector3.up);
            playerObj.rotation = Quaternion.Slerp(
                playerObj.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}
