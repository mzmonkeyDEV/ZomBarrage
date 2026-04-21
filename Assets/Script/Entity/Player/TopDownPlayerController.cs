using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class TopDownPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 12f;

    [Header("Input Setup")]
    [Tooltip("Drag your PlayerControls Input Action Asset's 'Move' action here.")]
    public InputActionReference moveAction;

    private CharacterController controller;
    private Vector2 moveInput;

    private void Awake()
    {
        // Get the CharacterController component automatically
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        // Enable the input action when the script is active
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        // Disable the input action when the script is inactive
        if (moveAction != null)
        {
            moveAction.action.Disable();
        }
    }

    private void Update()
    {
        // Read the Vector2 value from the joystick
        moveInput = moveAction.action.ReadValue<Vector2>();

        MovePlayer();
    }

    private void MovePlayer()
    {
        // Convert 2D joystick input into 3D world movement (X and Z axes for top-down)
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        // Apply movement using the CharacterController
        // We add a tiny bit of downward force (-0.1f) to keep the character grounded
        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = -0.1f;

        controller.Move(velocity * Time.deltaTime);

        // Rotate the character to face the direction they are moving
        if (moveDirection != Vector3.zero)
        {
            // Calculate the target rotation
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            // Smoothly rotate towards the target using Slerp
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}