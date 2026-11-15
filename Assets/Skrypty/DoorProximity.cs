using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DoorProximity : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(0.1f)] private float openRange = 0.8f;
    [SerializeField, Min(0.1f)] private float closeRange = 1f;
    [SerializeField, Min(1f)] private float framesPerSecond = 10f;

    private SpriteRenderer doorRenderer;
    private float animationFrame;
    private bool shouldOpen;

    private void Awake()
    {
        if (player == null || frames == null || frames.Length < 2)
        {
            Debug.LogError("Assign the player and at least two door sprites.", this);
            enabled = false;
            return;
        }

        doorRenderer = GetComponent<SpriteRenderer>();
        doorRenderer.sprite = frames[0];
    }

    private void Update()
    {
        if (player == null)
            return;

        Vector2 offset = (Vector2)player.position - (Vector2)transform.position;
        float range = shouldOpen ? Mathf.Max(openRange, closeRange) : openRange;
        shouldOpen = offset.sqrMagnitude <= range * range;

        int targetFrame = shouldOpen ? frames.Length - 1 : 0;
        animationFrame = Mathf.MoveTowards(
            animationFrame,
            targetFrame,
            framesPerSecond * Time.deltaTime
        );

        doorRenderer.sprite = frames[Mathf.RoundToInt(animationFrame)];
    }
}