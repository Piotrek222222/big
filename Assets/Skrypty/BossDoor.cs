using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BossDoor : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject bossLabel;
    [SerializeField, Min(0.1f)] private float interactionRange = 1.2f;

    private void OnEnable()
    {
        UpdateBossLabel();
    }

    private void Update()
    {
        GameRun run = GameRun.Instance;
        bool doorUnlocked = IsDoorUnlocked(run);

        SetBossLabelVisible(doorUnlocked);

        if (!doorUnlocked || player == null)
            return;

        Vector2 offset = (Vector2)(player.position - transform.position);
        bool playerInRange =
            offset.sqrMagnitude <= interactionRange * interactionRange;

        if (playerInRange && PressedE())
            run.EnterBossFight();
    }

    private void UpdateBossLabel()
    {
        SetBossLabelVisible(IsDoorUnlocked(GameRun.Instance));
    }

    private static bool IsDoorUnlocked(GameRun run)
    {
        return run != null && run.AwaitingPokerBoss && !run.IsFinished;
    }

    private void SetBossLabelVisible(bool visible)
    {
        if (bossLabel != null && bossLabel.activeSelf != visible)
            bossLabel.SetActive(visible);
    }

    private static bool PressedE()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}