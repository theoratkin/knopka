using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public delegate void OnPlayerDeath();

    public event OnPlayerDeath OnPlayerDeathEvent;

    public InputActionReference UseAction;

    public float FallingY = -100f;

    private const float ButtonReachDistance = 3f;

    private GameObject crosshair;

    public new Camera camera { get; private set; }

    public FirstPerson Controller { get; private set; }

    public Transform Checkpoint { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        //crosshair = transform.Find("HeadJoint/Player Camera/AutoCrosshair/Crosshair").gameObject;
        Controller = GetComponent<FirstPerson>();
        camera = Controller.Camera;

        UseAction.action.started += OnUse;
        UseAction.action.Enable();
    }

    public void SetCrosshairActive(bool state)
    {
        //crosshair.gameObject.SetActive(state);
    }

    void ResetPosition()
    {
        Controller.RotateCamera((Vector2)Checkpoint.eulerAngles);
        transform.position = Checkpoint.position;
        Controller.SkipMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < FallingY)
        {
            OnPlayerDeathEvent?.Invoke();
            ResetPosition();
        }
    }

    private void OnUse(InputAction.CallbackContext obj)
    {
        Button button = GetButton();
        if (button)
        {
            button.Press();
        }
    }


    Button GetButton()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        bool hasHit = Physics.Raycast(camera.transform.position,
                            camera.transform.TransformDirection(Vector3.forward),
                            out hit, ButtonReachDistance, LayerMask.GetMask("Button"), QueryTriggerInteraction.Collide
        );
        if (hasHit)
            return hit.transform.GetComponent<Button>();
        
        return null;
    }

    void FixedUpdate()
    {
    }
}
