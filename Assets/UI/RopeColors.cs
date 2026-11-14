using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeColors : MonoBehaviour
{
    public Color ropeAColor = Color.red;
    public Color ropeBColor = Color.blue;
    void Start()
    {
        ropeAColor = ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("RopeAColor", "FF0000FF"), out Color colorA) ? colorA : Color.red;
        ropeBColor = ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("RopeBColor", "0000FFFF"), out Color colorB) ? colorB : Color.blue;
        GetComponent<GrappleController>().ropeAColor = ropeAColor;
        GetComponent<GrappleController>().ropeBColor = ropeBColor;
    }
}
