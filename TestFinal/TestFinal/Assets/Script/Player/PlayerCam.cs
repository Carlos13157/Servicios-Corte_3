using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour{
    public PlayerInput playerInput;
    private InputAction moveAction;

    [SerializeField] private bool canUpdate = false;

    public Transform Target;

    private bool hasPlayerInput = false;

    public bool CanUpdate {get{return canUpdate;} set{
            canUpdate = value;
            Cursor.visible = !canUpdate;
            Cursor.lockState = canUpdate ? CursorLockMode.Locked : CursorLockMode.None;
            Debug.Log($"Can update: {canUpdate}");
        }
    }

    [SerializeField] public float sens;
    private float verticalRotation;
    private float horizontalRotation;

    void Start()
    {
        
    }

    void LateUpdate(){
        hasPlayerInput = playerInput != null;
        if (!hasPlayerInput){
            return;
        }
        
        Setup();
        if (canUpdate){
            UpdateCamMovement();
        }
    }

    void Setup()
    {
        moveAction = playerInput.actions.FindAction("CameraMove");
        CanUpdate = true;
    }

    void UpdateCamMovement()
    {
        if (Target == null)
        {
            return;
        }

        transform.position = Target.position;

        float mouseX = moveAction.ReadValue<Vector2>().x * sens;
        float mouseY = moveAction.ReadValue<Vector2>().y * sens;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        horizontalRotation += mouseX;

        Target.parent.Rotate(Vector3.up * mouseX);
        transform.rotation = Quaternion.Euler(verticalRotation, Target.parent.rotation.eulerAngles.y, 0);
    }

    public float GetMouseXRotation() {
        return verticalRotation;
    }
}
