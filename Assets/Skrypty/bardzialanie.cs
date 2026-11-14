using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarShop : MonoBehaviour
{
    public enum PotionType
    {
        Lucky,
        Money,
        Death
    }

    [System.Serializable]
    private class Bottle
    {
        public Button button;
        public PotionType type;
        [Min(1)] public int price = 10;
    }

    [SerializeField] private Bottle[] bottles = new Bottle[0];
    [SerializeField] private TMP_Text messageText;

    private void Start()
    {
        GameRun run = GameRun.Instance;

        for (int i = 0; i < bottles.Length; i++)
        {
            int index = i;
            Button button = bottles[index]?.button;

            if (button == null)
                continue;

            button.gameObject.SetActive(run == null || !run.IsBottleSold(index));
            button.onClick.AddListener(() => Buy(index));
        }
    }

    private void Buy(int index)
    {
        GameRun run = GameRun.Instance;

        if (run == null || !run.CasinoSessionActive || run.SecondsLeft <= 0f)
        {
            ShowMessage("Start an active run from the main menu.");
            return;
        }

        if (run.IsBottleSold(index))
            return;

        Bottle bottle = bottles[index];

        if (!run.TrySpend(bottle.price))
        {
            ShowMessage($"You need ${bottle.price} to buy this potion.");
            return;
        }

        run.MarkBottleSold(index);
        bottle.button.gameObject.SetActive(false);

        switch (bottle.type)
        {
            case PotionType.Lucky:
                run.ActivateLuckyPotion();
                ShowMessage("Lucky potion: +30 seconds of double luck.");
                break;

            case PotionType.Money:
                run.ActivateMoneyPotion();
                ShowMessage("Money potion: +30 seconds of 1.5x payouts.");
                break;

            case PotionType.Death:
                if (Random.Range(0, 4) == 0)
                {
                    run.EndRunFromDeathPotion();
                }
                else
                {
                    run.DoublePermanentLuck();
                    ShowMessage("You survived! Your luck multiplier doubled.");
                }
                break;
        }
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }
}