using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPerson : MonoBehaviour
{
    public InputActionReference LookAction;
    public InputActionReference MoveAction;
    public InputActionReference JumpAction;

    public Transform Head;
    public Camera Camera;
    public GameObject crosshair;

    [Range(1f, 100f)]
    public float MovementSpeed = .5f;
    [Range(1f, 20f)]
    public float MouseSensitivity = .5f;

    [Range(1f, 100f)]
    public float Gravity = .5f;
    [Range(1f, 100f)]
    public float JumpForce = 10f;

    [Range(1f, 100f)]
    public float Mass = 10f;

    public bool SkipMove { get; set; }

    private CharacterController controller;

    private Vector3 moveDirection;

    private bool jumpStartedPrev;

    void Start()
    {
        SetActive(true);

        controller = GetComponent<CharacterController>();
    }

    public void SetActive(bool state)
    {
        if (state)
        {
            LookAction.action.Enable();
            MoveAction.action.Enable();
            JumpAction.action.Enable();
        }
        else
        {
            LookAction.action.Disable();
            MoveAction.action.Disable();
            JumpAction.action.Disable();
        }

        Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
    }

    void Update()
    {
        OnMove();
        OnLook();
    }


    #region Movement

    void OnMove()
    {
        Vector2 delta = MoveAction.action.ReadValue<Vector2>() * MovementSpeed;
        float moveDirectionY = moveDirection.y;
        moveDirection = Head.transform.forward * delta.y + Head.transform.right * delta.x;

        if (Jumped() && controller.isGrounded)
            moveDirection.y = JumpForce;
        else
            moveDirection.y = moveDirectionY;

        if (!controller.isGrounded)
            moveDirection.y -= Gravity * Time.deltaTime;

        if (SkipMove)
        {
            SkipMove = false;
            return;
        }

        controller.Move(moveDirection * Time.deltaTime);
    }

    private bool Jumped()
    {
        bool started = JumpAction.action.phase == InputActionPhase.Started;
        // Jump.
        if (started && !jumpStartedPrev)
        {
            jumpStartedPrev = started;
            return true;
        }
        jumpStartedPrev = started;
        return false;
    }

    #endregion


    #region Looking

    void OnLook()
    {
        Vector2 delta = LookAction.action.ReadValue<Vector2>() * MouseSensitivity * Time.deltaTime * 10f;
        Head.transform.localEulerAngles += new Vector3(0f, delta.x, 0f);

        float angle = Camera.transform.localEulerAngles.x;
        if (angle > 180f)
            angle -= 360f;
        if (angle - delta.y > 90f || angle - delta.y < -90f)
            return;

        Camera.transform.localEulerAngles += new Vector3(-delta.y, 0f, 0f);
    }

    public void RotateCamera(Vector2 rotation)
    {
        Head.transform.localEulerAngles = new Vector3(0f, rotation.y, 0f);
        Camera.transform.localEulerAngles = new Vector3(rotation.x, 0f, 0f);
    }

    #endregion


    public void SetCrosshairActive(bool state)
    {
        crosshair.SetActive(state);
    }
}
