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

    private float verticalVelocity;
    private float currentJumpForce = 0f;
    private float currentGravity = 0f;

    private bool isGroundedPrev;
    private bool jumpStartedPrev;
    private bool jumped = false;

    // Start is called before the first frame update
    void Start()
    {
        SetActive(true);

        controller = GetComponent<CharacterController>();

        verticalVelocity = -Gravity;
        isGroundedPrev = controller.isGrounded;
        jumpStartedPrev = JumpAction.action.phase == InputActionPhase.Started;
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

    private void FixedUpdate()
    {
        CalculateVerticalVelocity();

        OnMove();
    }

    // Update is called once per frame
    void Update()
    {
        OnLook();
    }


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


    #region Movement

    void OnMove()
    {
        if (SkipMove)
        {
            SkipMove = false;
            return;
        }
        Vector2 delta = MoveAction.action.ReadValue<Vector2>() * MovementSpeed;
        Vector3 direction = Head.transform.forward * delta.y + Head.transform.right * delta.x;
        direction.y = verticalVelocity;
        controller.Move(direction * Time.deltaTime);
    }

    #endregion


    #region Vertical Speed

    void CalculateVerticalVelocity()
    {
        // Jump.
        if (JumpAction.action.phase == InputActionPhase.Started && !jumpStartedPrev)
        {
            if (controller.isGrounded)
            {
                currentJumpForce = JumpForce;
                jumped = true;
            }
        }
        jumpStartedPrev = JumpAction.action.phase == InputActionPhase.Started;

        // Losing ground.
        if (!controller.isGrounded && isGroundedPrev)
        {
            if (!jumped)
                currentGravity = 0f;
        }
        // Gaining ground.
        if (controller.isGrounded && !isGroundedPrev)
        {
            jumped = false;
            currentGravity = Gravity;
        }
        isGroundedPrev = controller.isGrounded;

        if (currentJumpForce > 0f)
            currentJumpForce -= .01f * Mass;

        if (!controller.isGrounded && currentGravity < Gravity)
            currentGravity += .01f * Mass;

        verticalVelocity = -currentGravity + currentJumpForce;
    }

    #endregion

    public void SetCrosshairActive(bool state)
    {
        crosshair.SetActive(state);
    }
}
