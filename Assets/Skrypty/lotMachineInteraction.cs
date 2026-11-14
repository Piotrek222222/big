using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Tilemap))]
public class SlotMachineInteraction : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private TileBase machineTile;
    [SerializeField, Min(0.1f)] private float interactionRange = 0.8f;
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.35f);
    [SerializeField] private string targetScene = "SlotMachine";

    private Tilemap tilemap;
    private readonly List<MachineCell> machines = new List<MachineCell>();

    private class MachineCell
    {
        public Vector3Int Position;
        public Color OriginalColor;
        public bool Highlighted;
    }

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();

        if (player == null || machineTile == null)
        {
            //Debug.LogError("Przypisz Player i Machine Tile w Inspectorze.", this);
            enabled = false;
            return;
        }

        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.GetTile(position) != machineTile)
                continue;

            machines.Add(new MachineCell
            {
                Position = position,
                OriginalColor = tilemap.GetColor(position)
            });

            tilemap.RemoveTileFlags(position, TileFlags.LockColor);
        }
    }

    private void Update()
    {
        bool canInteract = false;
        float rangeSquared = interactionRange * interactionRange;

        foreach (MachineCell machine in machines)
        {
            Vector2 center = tilemap.GetCellCenterWorld(machine.Position);
            Vector2 offset = center - (Vector2)player.position;
            bool inRange = offset.sqrMagnitude <= rangeSquared;
            canInteract |= inRange;

            if (machine.Highlighted == inRange)
                continue;

            machine.Highlighted = inRange;
            tilemap.SetColor(
                machine.Position,
                inRange ? highlightColor : machine.OriginalColor
            );
        }

        if (!canInteract || !PressedE())
            return;

        if (!Application.CanStreamedLevelBeLoaded(targetScene))
        {
            Debug.LogError($"Dodaj scenę '{targetScene}' do Build Profiles → Scene List.", this);
            return;
        }

        SceneManager.LoadScene(targetScene);
    }

    private void OnDisable()
    {
        foreach (MachineCell machine in machines)
        {
            if (!machine.Highlighted)
                continue;

            tilemap.SetColor(machine.Position, machine.OriginalColor);
            machine.Highlighted = false;
        }
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