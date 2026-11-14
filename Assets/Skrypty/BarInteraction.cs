using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Tilemap))]
public class BarInteraction : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField, Min(0.1f)] private float interactionRange = 0.9f;
    [SerializeField] private Color highlightColor = Color.yellow;

    private const string SceneName = "Bar";

    private Tilemap barTilemap;
    private bool highlighted;
    private readonly Dictionary<Vector3Int, Color> originalColors = new();

    private void Awake()
    {
        barTilemap = GetComponent<Tilemap>();

        if (player == null)
        {
            Debug.LogError("Przeciągnij Gracz do pola Player.", this);
            enabled = false;
            return;
        }

        foreach (Vector3Int cell in barTilemap.cellBounds.allPositionsWithin)
        {
            if (barTilemap.HasTile(cell))
                originalColors.Add(cell, barTilemap.GetColor(cell));
        }
    }

    private void Update()
    {
        bool inRange = false;
        Vector2 playerPosition = player.position;
        float rangeSquared = interactionRange * interactionRange;

        foreach (Vector3Int cell in originalColors.Keys)
        {
            Vector2 cellCenter = barTilemap.GetCellCenterWorld(cell);

            if ((cellCenter - playerPosition).sqrMagnitude <= rangeSquared)
            {
                inRange = true;
                break;
            }
        }

        SetHighlight(inRange);

        if (!inRange || !PressedE())
            return;

        if (!Application.CanStreamedLevelBeLoaded(SceneName))
        {
            Debug.LogError("Dodaj scenę Bar do Build Profiles → Scene List.", this);
            return;
        }

        SceneManager.LoadScene(SceneName);
    }

    private void OnDisable()
    {
        SetHighlight(false);
    }

    private void SetHighlight(bool active)
    {
        if (highlighted == active || barTilemap == null)
            return;

        foreach (var tile in originalColors)
        {
            barTilemap.RemoveTileFlags(tile.Key, TileFlags.LockColor);
            barTilemap.SetColor(tile.Key, active ? highlightColor : tile.Value);
        }

        highlighted = active;
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