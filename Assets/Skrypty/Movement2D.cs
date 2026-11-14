using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 3f;

    private Rigidbody2D body;
    private Vector2 movement;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        bool right = keyboard != null && (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed);
        bool left = keyboard != null && (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed);
        bool up = keyboard != null && (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed);
        bool down = keyboard != null && (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed);
#else
        bool right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        bool left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool down = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
#endif
        float x = (right ? 1 : 0) - (left ? 1 : 0);
        float y = (up ? 1 : 0) - (down ? 1 : 0);
        movement = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        body.linearVelocity = movement * speed;
    }
}