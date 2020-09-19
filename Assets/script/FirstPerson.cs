// First Person controller based on Character Controller and the New Input System.
// Can handle moving platforms without messing with hierarchy.
//
// Some code is taken from these tutorials by nsdgmax:
// https://sharpcoderblog.com/blog/unity-3d-fps-controller
// https://sharpcoderblog.com/blog/unity-3d-character-controller-moving-platform-support

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPerson : MonoBehaviour
{
    #region Inspector

    [Header("Input")]
    public InputActionReference LookAction;
    public InputActionReference MoveAction;
    public InputActionReference JumpAction;

    [Header("Objects")]
    public Transform Head;
    public Camera Camera;
    public GameObject crosshair;

    [Header("Mouse")]
    [Range(1f, 20f)]
    public float MouseSensitivity = 3f;

    [Header("Movement")]
    [Range(1f, 100f)]
    public float MovementSpeed = 10f;
    [Range(1f, 100f)]
    public float Gravity = 20f;

    [Header("Jumping")]
    [Range(1f, 100f)]
    public float JumpForce = 8f;
    [Range(0f, 1f)]
    public float JumpBuffer = 0.3f;
    [Range(0f, 1f)]
    public float CyoteTime = 0.2f;
    public string PlatformTag = "Platform";

    #endregion


    #region Private Fields

    CharacterController controller;

    Vector3 moveDirection;

    bool active = true;
    bool isGroundedPrev;
    bool jumpStartedPrev;

    float jumpBuffer = 0f;
    float cyoteTime = 0f;

    Transform activePlatform;
    Vector3 externalMovement = Vector3.zero;
    Vector3 activeGlobalPlatformPoint;
    Vector3 activeLocalPlatformPoint;
    Quaternion activeGlobalPlatformRotation;
    Quaternion activeLocalPlatformRotation;

    #endregion


    #region Properties

    public bool SkipMove { get; set; }

    public bool Active
    {
        get { return active; }
        set
        {
            active = value;
            if (value)
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

            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
    }

    public bool CrosshairActive
    {
        get { return crosshair.activeSelf; }
        set { crosshair.SetActive(value); }
    }

    #endregion


    #region Initialization

    void Start()
    {
        Active = true;

        controller = GetComponent<CharacterController>();
        isGroundedPrev = controller.isGrounded;
    }

    #endregion


    #region Update

    void Update()
    {
        OnLook();
    }

    void LateUpdate()
    {
        OnMove();
    }

    #endregion


    #region Movement

    void OnMove()
    {
        Vector2 delta = MoveAction.action.ReadValue<Vector2>() * MovementSpeed;
        float moveDirectionY = moveDirection.y;
        moveDirection = Head.transform.forward * delta.y + Head.transform.right * delta.x;

        bool jumped = Jumped();
        bool grounded = controller.isGrounded && !isGroundedPrev;
        bool ungrounded = !controller.isGrounded && isGroundedPrev;
        isGroundedPrev = controller.isGrounded;

        if (grounded)
            moveDirectionY = -1f;
        if (ungrounded)
            cyoteTime = CyoteTime;

        if (jumped && !controller.isGrounded)
            jumpBuffer = JumpBuffer;

        if ((jumped || jumpBuffer > 0f) && (controller.isGrounded || cyoteTime > 0f))
        {
            moveDirection.y = JumpForce;
            jumpBuffer = 0f;
        }
        else
        {
            moveDirection.y = moveDirectionY;
        }

        if (jumpBuffer > 0f)
            jumpBuffer -= Time.deltaTime;
        if (cyoteTime > 0f)
            cyoteTime -= Time.deltaTime;

        if (!controller.isGrounded)
            moveDirection.y -= Gravity * Time.deltaTime;

        Vector3 extrn = Vector3.zero;
        extrn = GetExternalMovement(jumped);

        if (SkipMove)
        {
            SkipMove = false;
            return;
        }

        controller.Move(moveDirection * Time.deltaTime + extrn);
    }

    bool Jumped()
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


    #region Platforms

    void UpdateMovingPlatform()
    {
        activeGlobalPlatformPoint = transform.position;
        activeLocalPlatformPoint = activePlatform.InverseTransformPoint(transform.position);
        activeGlobalPlatformRotation = transform.rotation;
        activeLocalPlatformRotation = Quaternion.Inverse(activePlatform.rotation) * transform.rotation;
    }

    float HeightFromPlatform()
    {
        if (!activePlatform)
            return 0f;
        return Mathf.Abs(activePlatform.position.y - transform.position.y);
    }

    Vector3 GetExternalMovement(bool jumped)
    {
        Vector3 extrn = Vector3.zero;
        if (activePlatform != null)
        {
            Vector3 newGlobalPlatformPoint = activePlatform.TransformPoint(activeLocalPlatformPoint);
            externalMovement = newGlobalPlatformPoint - activeGlobalPlatformPoint;
            if (externalMovement.magnitude > 0.01f)
            {
                extrn = externalMovement;
                Physics.SyncTransforms();
            }
            if (activePlatform)
            {
                // Support moving platform rotation
                Quaternion newGlobalPlatformRotation = activePlatform.rotation * activeLocalPlatformRotation;
                Quaternion rotationDiff = newGlobalPlatformRotation * Quaternion.Inverse(activeGlobalPlatformRotation);
                // Prevent rotation of the local up vector
                rotationDiff = Quaternion.FromToRotation(rotationDiff * Vector3.up, Vector3.up) * rotationDiff;
                transform.rotation = rotationDiff * transform.rotation;
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

                UpdateMovingPlatform();
            }
        }
        else
        {
            if (externalMovement.magnitude > 0.01f)
            {
                externalMovement = Vector3.Lerp(externalMovement, Vector3.zero, Time.deltaTime);
                extrn = externalMovement;
                Physics.SyncTransforms();
            }
        }

        return extrn;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Make sure we are really standing on a straight *new* platform.
        // Not on the underside of one and not falling down from it either!
        if (hit.collider.tag == PlatformTag && hit.moveDirection.y < -0.9 && hit.normal.y > 0.41)
        {
            if (activePlatform != hit.collider.transform)
            {
                activePlatform = hit.collider.transform;
                UpdateMovingPlatform();
            }
        }
        else
        {
            activePlatform = null;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == PlatformTag)
        {
            activePlatform = null;
            externalMovement = Vector3.zero;
        }
    }

    #endregion
}
