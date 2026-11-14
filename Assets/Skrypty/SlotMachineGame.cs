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

    private const int SpinCost = 10;
    private const int WinPayout = 300;

    private bool spinning;

    private void Start()
    {
        if (leverUp == null || leverDown == null ||
            symbol1 == null || symbol2 == null || symbol3 == null ||
            resultText == null || symbols == null || symbols.Length == 0 ||
            System.Array.Exists(symbols, sprite => sprite == null))
        {
            Debug.LogError("Assign all SlotMachineGame fields in the Inspector.", this);
            enabled = false;
            return;
        }

        leverUp.SetActive(true);
        leverDown.SetActive(false);
        resultText.gameObject.SetActive(false);
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        bool spacePressed = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
#else
        bool spacePressed = Input.GetKeyDown(KeyCode.Space);
#endif

        if (!spacePressed || spinning)
            return;

        GameRun run = GameRun.Instance;

        if (run == null || run.Day == 0)
        {
            ShowResult("<size=40%>Start from the main menu.</size>");
            return;
        }

        if (!run.CasinoSessionActive || run.SecondsLeft <= 0f)
        {
            ShowResult("<size=40%>The casino session has ended.</size>");
            return;
        }

        if (!run.TrySpend(SpinCost))
        {
            ShowResult($"<size=70%>You need ${SpinCost} to play.</size>");
            return;
        }

        Sprite first = symbols[Random.Range(0, symbols.Length)];
        Sprite second = symbols[Random.Range(0, symbols.Length)];
        Sprite third = symbols[Random.Range(0, symbols.Length)];

        bool win = first == second && second == third;
        int payout = win ? run.CreditWin(WinPayout) : 0;

        StartCoroutine(RevealSymbols(first, second, third, payout));
    }

    private IEnumerator RevealSymbols(Sprite first, Sprite second, Sprite third, int payout)
    {
        spinning = true;

        leverUp.SetActive(false);
        leverDown.SetActive(true);
        symbol1.enabled = false;
        symbol2.enabled = false;
        symbol3.enabled = false;
        resultText.gameObject.SetActive(false);

        var delay = new WaitForSecondsRealtime(1f);

        yield return delay;
        Reveal(symbol1, first);

        yield return delay;
        Reveal(symbol2, second);

        yield return delay;
        Reveal(symbol3, third);

        ShowResult(payout > 0 ? $"BIG WIN! +${payout}" : "YOU LOSE!");

        leverDown.SetActive(false);
        leverUp.SetActive(true);
        spinning = false;
    }

    private void Reveal(Image image, Sprite sprite)
    {
        image.sprite = sprite;
        image.enabled = true;
    }

    private void ShowResult(string message)
    {
        resultText.text = message;
        resultText.gameObject.SetActive(true);
    }
}