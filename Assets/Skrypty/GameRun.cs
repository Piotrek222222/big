using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRun : MonoBehaviour
{
    public static GameRun Instance { get; private set; }

    [SerializeField, Min(1f)] private float sessionDuration = 180f;

    private const int StartingMoney = 60;
    private const int DaysBetweenPayments = 3;
    private const double PotionDuration = 30d;
    private static readonly int[] Payments = { 100, 175, 275 };

<<<<<<< Updated upstream
    // Intermission screen shown after every in-game day.
    public const string NowyDzienScene = "NowyDzien";
=======
    private readonly HashSet<int> soldBottles = new HashSet<int>();
>>>>>>> Stashed changes

    private string casinoScene;
    private double sessionEndsAt;
    private bool startTimerOnCasinoLoad;

    private double luckyEndsAt;
    private double moneyEndsAt;
    private float permanentLuckMultiplier = 1f;

    public int Day { get; private set; }
    public bool IsFinished { get; private set; }
    public bool CasinoSessionActive { get; private set; }
    public string Message { get; private set; } = "";

    public int Money => GameData.PobierzLubUtworz().SaldoGracza;
    public int Cash => Money;

    public int NextDueDay => Day == 0 || IsFinished
        ? 0
        : ((Day - 1) / DaysBetweenPayments + 1) * DaysBetweenPayments;

    public int NextPayment => Day == 0 || IsFinished
        ? 0
        : Payments[(Day - 1) / DaysBetweenPayments];

    public float SecondsLeft => CasinoSessionActive
        ? Mathf.Max(0f, (float)(sessionEndsAt - Time.unscaledTimeAsDouble))
        : 0f;

    public float EffectiveLuck
    {
        get
        {
            float baseLuck = GameData.PobierzLubUtworz().Luck;
            float luckyMultiplier = Time.unscaledTimeAsDouble < luckyEndsAt ? 2f : 1f;
            return baseLuck * permanentLuckMultiplier * luckyMultiplier;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    private void Update()
    {
        if (CasinoSessionActive && SecondsLeft <= 0f)
            FinishDay();
    }

    public void StartNewRun(string startingCasinoScene)
    {
        casinoScene = startingCasinoScene;
        Day = 1;
        IsFinished = false;
        CasinoSessionActive = false;
        startTimerOnCasinoLoad = true;
<<<<<<< HEAD

        soldBottles.Clear();
        luckyEndsAt = 0d;
        moneyEndsAt = 0d;
        permanentLuckMultiplier = 1f;

        GameData.PobierzLubUtworz().SaldoGracza = StartingMoney;
        Message = "Your first allowance payment is due at the end of day 3.";
=======
        Message = "";
>>>>>>> 5f1132f (Add NowyDzien scene)
    }

    public bool TrySpend(int amount)
    {
        if (!CasinoSessionActive || SecondsLeft <= 0f || amount <= 0)
            return false;

        GameData data = GameData.PobierzLubUtworz();

        if (data.SaldoGracza < amount)
            return false;

        data.SaldoGracza -= amount;
        return true;
    }

    public void AddCash(int amount)
    {
        if (!CasinoSessionActive || amount <= 0)
            return;

        GameData data = GameData.PobierzLubUtworz();
        data.SaldoGracza += amount;
    }

    public int CreditWin(int payout)
    {
        if (!CasinoSessionActive || payout <= 0)
            return 0;

        int credited = payout;

        if (Time.unscaledTimeAsDouble < moneyEndsAt)
            credited = (int)Math.Round(payout * 1.5d, MidpointRounding.AwayFromZero);

        AddCash(credited);
        return credited;
    }

    public bool IsBottleSold(int index) => soldBottles.Contains(index);

    public void MarkBottleSold(int index)
    {
        soldBottles.Add(index);
    }

    public void ActivateLuckyPotion()
    {
        luckyEndsAt = Math.Max(luckyEndsAt, Time.unscaledTimeAsDouble)
                      + PotionDuration;
    }

    public void ActivateMoneyPotion()
    {
        moneyEndsAt = Math.Max(moneyEndsAt, Time.unscaledTimeAsDouble)
                      + PotionDuration;
    }

    public void DoublePermanentLuck()
    {
        permanentLuckMultiplier *= 2f;
    }

    public void EndRunFromDeathPotion()
    {
        if (!CasinoSessionActive || IsFinished)
            return;

        IsFinished = true;
        CasinoSessionActive = false;
        startTimerOnCasinoLoad = false;
        Message = "GAME OVER! The Death potion ended your run.";
        SceneManager.LoadScene(casinoScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!startTimerOnCasinoLoad || scene.name != casinoScene)
            return;

        startTimerOnCasinoLoad = false;
        sessionEndsAt = Time.unscaledTimeAsDouble + sessionDuration;
        CasinoSessionActive = true;
    }

    private void FinishDay()
    {
        CasinoSessionActive = false;
        Message = "Time's up. A new day begins.";

        if (Day == NextDueDay)
        {
            int payment = NextPayment;
            GameData data = GameData.PobierzLubUtworz();

            if (data.SaldoGracza < payment)
            {
                IsFinished = true;
                Message = $"GAME OVER! You are ${payment - data.SaldoGracza} short.";
            }
            else
            {
                data.SaldoGracza -= payment;
                IsFinished = Day == Payments.Length * DaysBetweenPayments;
                Message = IsFinished
                    ? "YOU WIN! You paid all the allowances."
                    : $"Allowance paid: ${payment}.";
            }
        }

        if (!IsFinished)
        {
            Day++;
            startTimerOnCasinoLoad = true;
        }

        // Go through the intermission screen. It continues to the casino,
        // where the timer for the next day starts (startTimerOnCasinoLoad).
        SceneManager.LoadScene(
            Application.CanStreamedLevelBeLoaded(NowyDzienScene)
                ? NowyDzienScene
                : casinoScene);
    }

    /// <summary>
    /// Called by the intermission screen ("NowyDzien") to start the next
    /// casino day. The day timer starts once the casino scene has loaded.
    /// </summary>
    public bool ContinueDay()
    {
        if (IsFinished || CasinoSessionActive)
            return false;

        if (!Application.CanStreamedLevelBeLoaded(casinoScene))
        {
            Debug.LogError($"Add scene '{casinoScene}' to the Build Profiles scene list.", this);
            return false;
        }

        SceneManager.LoadScene(casinoScene);
        return true;
    }
}
