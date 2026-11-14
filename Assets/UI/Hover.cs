using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    public List<GameObject> UI = new List<GameObject>();

    // Lista, która przechowuje oryginalną skalę każdego obiektu
    private List<Vector3> originalScales = new List<Vector3>();

    private void Start()
    {
        // Zapamiętanie początkowej skali każdego elementu UI
        foreach (var obj in UI)
        {
            originalScales.Add(obj.transform.localScale);
        }
    }

    public void HoverEnter(int index)
    {
        if (index < 0 || index >= UI.Count) return;

        Vector3 baseScale = originalScales[index];

        // Powiększenie o 0.1
        Vector3 enterScale = new Vector3(
            baseScale.x + 0.1f,
            baseScale.y + 0.1f,
            baseScale.z + 0.1f
        );

        UI[index].transform.localScale = enterScale;
    }

    public void HoverExit(int index)
    {
        if (index < 0 || index >= UI.Count) return;

        // Przywrócenie oryginalnej skali
        UI[index].transform.localScale = originalScales[index];
    }
}
