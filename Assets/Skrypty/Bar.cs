using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Tilemap))]
public class Bar : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField, Min(0.1f)] private float interactionRange = 0.9f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private string sceneName = "Bar";

    private Tilemap barTilemap;
    private bool highlighted;

    private readonly Dictionary<Vector3Int, Color> originalColors =
        new Dictionary<Vector3Int, Color>();

    private void Awake()
    {
        barTilemap = GetComponent<Tilemap>();

        if (player == null)
        {
            Debug.LogError("Assign the player in the Bar component.", this);
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
        bool nearBar = false;
        Vector2 playerPosition = player.position;
        float rangeSquared = interactionRange * interactionRange;

        foreach (Vector3Int cell in originalColors.Keys)
        {
            Vector2 center = barTilemap.GetCellCenterWorld(cell);

            if ((center - playerPosition).sqrMagnitude <= rangeSquared)
            {
                nearBar = true;
                break;
            }
        }

        SetHighlight(nearBar);

        if (!nearBar || !PressedE())
            return;

        if (string.IsNullOrWhiteSpace(sceneName) ||
            !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"Add scene '{sceneName}' to the Build Profiles scene list.",
                this
            );
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void OnDisable()
    {
        SetHighlight(false);
    }

    private void SetHighlight(bool active)
    {
        if (barTilemap == null || highlighted == active)
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
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}