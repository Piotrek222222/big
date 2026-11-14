using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SlotMachineGame : MonoBehaviour
{
    [SerializeField] private GameObject leverUp;
    [SerializeField] private GameObject leverDown;

    [SerializeField] private Image symbol1;
    [SerializeField] private Image symbol2;
    [SerializeField] private Image symbol3;
    [SerializeField] private Sprite[] symbols;

    [SerializeField] private TMP_Text resultText;

    [SerializeField] private string luckPrefsKey = "PlayerLuck";
    [SerializeField] private float defaultLuck = 0f;

    private float luck;
    private bool spinning;

    private void Start()
    {
        if (leverUp == null || leverDown == null ||
            symbol1 == null || symbol2 == null || symbol3 == null ||
            resultText == null || symbols == null || symbols.Length == 0 ||
            System.Array.Exists(symbols, sprite => sprite == null))
        {
            Debug.LogError("Uzupełnij wszystkie pola SlotMachineGame w Inspectorze.", this);
            enabled = false;
            return;
        }

        luck = PlayerPrefs.GetFloat(luckPrefsKey, defaultLuck);

        leverUp.SetActive(true);
        leverDown.SetActive(false);
        resultText.gameObject.SetActive(false);
        resultText.text = "";
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        bool spacePressed = Keyboard.current != null &&
                            Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        bool spacePressed = Input.GetKeyDown(KeyCode.Space);
#endif

        if (spacePressed && !spinning)
            StartCoroutine(RevealSymbols());
    }

    private IEnumerator RevealSymbols()
    {
        spinning = true;
        luck = PlayerPrefs.GetFloat(luckPrefsKey, defaultLuck);

        leverUp.SetActive(false);
        leverDown.SetActive(true);
        symbol1.enabled = false;
        symbol2.enabled = false;
        symbol3.enabled = false;
        resultText.text = "";

        var delay = new WaitForSecondsRealtime(1f);

        yield return delay;
        Reveal(symbol1);

        yield return delay;
        Reveal(symbol2);

        yield return delay;
        if (symbol1.sprite == symbol2.sprite && Random.Range(0f, 100f) < luck)
        {
            symbol3.sprite = symbol1.sprite;
            symbol3.enabled = true;
        }
        else
        {
            Reveal(symbol3);
        }

        bool win = symbol1.sprite == symbol2.sprite &&
                   symbol2.sprite == symbol3.sprite;

        resultText.text = win ? "BIG WIN!" : "YOU LOSE!";
        resultText.gameObject.SetActive(true);
        leverDown.SetActive(false);
        leverUp.SetActive(true);
        spinning = false;
    }

    private void Reveal(Image image)
    {
        image.sprite = symbols[Random.Range(0, symbols.Length)];
        image.enabled = true;
    }
}