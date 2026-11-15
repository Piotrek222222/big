using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Tilemap))]
public class BossTable : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField, Min(0.1f)] private float interactionRange = 0.9f;
    [SerializeField] private Color highlightColor = Color.yellow;

    private const string SceneName = "Dices";
    private Tilemap tables;

    private void Awake()
    {
        tables = GetComponent<Tilemap>();

        if (player == null)
        {
            Debug.LogError("Przypisz Gracz do pola Player.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        bool nearTable = false;
        Vector2 playerPosition = player.position;

        foreach (Vector3Int cell in tables.cellBounds.allPositionsWithin)
        {
            if (!tables.HasTile(cell))
                continue;

            Vector2 tablePosition = tables.GetCellCenterWorld(cell);
            bool inRange = Vector2.Distance(playerPosition, tablePosition) <= interactionRange;
            nearTable |= inRange;

            Color color = inRange ? highlightColor : Color.white;

            if (tables.GetColor(cell) != color)
            {
                tables.RemoveTileFlags(cell, TileFlags.LockColor);
                tables.SetColor(cell, color);
            }
        }

        if (!nearTable || !PressedE())
            return;

        if (!Application.CanStreamedLevelBeLoaded(SceneName))
        {
            Debug.LogError("Dodaj scenę BlackJack do Build Profiles → Scene List.", this);
            return;
        }

        SceneManager.LoadScene(SceneName);
    }

    private static bool PressedE()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}