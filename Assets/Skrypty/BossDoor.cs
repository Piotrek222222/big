using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BossDoor : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject bossLabel;
    [SerializeField, Min(0.1f)] private float interactionRange = 1.2f;

    private void Update()
    {
        GameRun run = GameRun.Instance;
        bool available = run != null && run.AwaitingPokerBoss && !run.IsFinished;

        if (bossLabel != null && bossLabel.activeSelf != available)
            bossLabel.SetActive(available);

        if (!available || player == null)
            return;

        Vector2 offset = (Vector2)(player.position - transform.position);

        if (offset.sqrMagnitude <= interactionRange * interactionRange && PressedE())
            run.EnterBossFight();
    }

    private static bool PressedE()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        return keyboard != null && keyboard.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}