using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private Animator animator;

    private static readonly int DirectionId = Animator.StringToHash("direction");

    private Rigidbody2D body;
    private Vector2 movement;
    private int previousDirection = -1;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning("Przypisz Animator gracza w polu Player Movement → Animator.", this);
        }
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        bool up = keyboard != null && (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed);
        bool down = keyboard != null && (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed);
        bool left = keyboard != null && (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed);
        bool right = keyboard != null && (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed);
#else
        bool up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool down = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
#endif

        int x = (right ? 1 : 0) - (left ? 1 : 0);
        int y = (up ? 1 : 0) - (down ? 1 : 0);
        movement = new Vector2(x, y).normalized;

        int direction = 0;

        if (y < 0)
            direction = 1; 
        else if (y > 0)
            direction = 2; 
        else if (x < 0)
            direction = 3; 
        else if (x > 0)
            direction = 4; 

        if (animator != null)
        {
            animator.SetInteger(DirectionId, direction);
        }

        if (direction != previousDirection)
        {
            string animatorName = animator != null ? animator.gameObject.name : "brak";
            Debug.Log($"Direction = {direction} | Animator: {animatorName}", this);
            previousDirection = direction;
        }
    }

    private void FixedUpdate()
    {
        body.linearVelocity = movement * speed;
    }
}