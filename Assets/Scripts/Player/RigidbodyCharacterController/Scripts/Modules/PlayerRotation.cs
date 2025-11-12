using System;
using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    private CharacterMotor motor;
    private GameObject camFollowTarget;

    private float yaw = 0f;
    private float pitch = 0f;

    private float deadzone = 0.02f;
    
    private void Awake()
    {
        motor = GetComponent<CharacterMotor>();
    }

    private void Start()
    {
        camFollowTarget = GameObject.Find("CamFollowTarget");
        
        yaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        /*motor.Rb.angularVelocity = Vector3.zero;

        Vector3 lookDirection = motor.RawMoveDirection;
        if (lookDirection.sqrMagnitude < float.Epsilon) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        
        Quaternion newRotation = Quaternion.Slerp(
            motor.Rb.rotation,
            targetRotation, 
            motor.MovementData.rotationSpeed * Time.fixedDeltaTime
        );
        
        motor.Rb.MoveRotation(newRotation);*/

        float mouseX = motor._playerInput.LookInput.x;
        float mouseY = motor._playerInput.LookInput.y;
        
        if (Mathf.Abs(mouseX) > deadzone || Mathf.Abs(mouseY) > deadzone)
        {
            mouseX *= motor.MovementData.rotationSpeed * Time.deltaTime;
            mouseY *= motor.MovementData.rotationSpeed * Time.deltaTime;

            yaw += mouseX;
            pitch -= mouseY; 
            pitch = Mathf.Clamp(pitch, -90f, 90f);
        }
        
        Quaternion targetBodyRotation = Quaternion.Euler(0f, yaw, 0f);

        motor.transform.localRotation = targetBodyRotation;
        
        camFollowTarget.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
