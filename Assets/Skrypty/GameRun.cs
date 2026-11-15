using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRun : MonoBehaviour
{
    public static GameRun Instance { get; private set; }

    [SerializeField, Min(1f)] private float sessionDuration = 180f;
    [SerializeField, Min(1)] private int startingQuota = 20;
    [SerializeField, Min(0)] private int bossReward = 50;
    [SerializeField] private string pokerBossScene = "BossBattle";
    [SerializeField] private string newDayScene = "NowyDzien";

    private const int DailyMoney = 50;
    private const float StartingLuck = 10f;
    private const int BossInterval = 3;
    private const int FinalDay = 9;
    private const double PotionDuration = 30d;

    private readonly HashSet<int> soldBottles = new HashSet<int>();

    private string casinoScene;
    private int dailyQuota;
    private double sessionEndsAt;
    private bool startTimerOnCasinoLoad;
    private bool waitingForNextDay;
    private bool enteringBossFight;

    private double luckyEndsAt;
    private double moneyEndsAt;
    private float permanentLuckMultiplier = 1f;
    private bool slotGuaranteeUsed;

    public int Day { get; private set; }
    public bool IsFinished { get; private set; }
    public bool CasinoSessionActive { get; private set; }
    public bool AwaitingPokerBoss { get; private set; }
    public string Message { get; private set; } = "";

    public int Money => GameData.PobierzLubUtworz().SaldoGracza;
    public int Cash => Money;
    public int BossReward => bossReward;

    public int NextDueDay =>
        Day > 0 && !IsFinished && !AwaitingPokerBoss ? Day : 0;

    public int NextPayment => NextDueDay > 0 ? dailyQuota : 0;

    public float SecondsLeft => CasinoSessionActive
        ? Mathf.Max(0f, (float)(sessionEndsAt - Time.unscaledTimeAsDouble))
        : 0f;

    public bool CanUseCasino => CasinoSessionActive &&
                                !IsFinished &&
                                !AwaitingPokerBoss &&
                                SecondsLeft > 0f;

    public float EffectiveLuck
    {
        get
        {
            float baseLuck = GameData.PobierzLubUtworz().Luck;
            float temporaryMultiplier =
                Time.unscaledTimeAsDouble < luckyEndsAt ? 2f : 1f;

            return baseLuck * permanentLuckMultiplier * temporaryMultiplier;
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
        if (Day == 0 || IsFinished)
            return;

        if (Money <= 0)
        {
            OnBalanceDepleted();
            return;
        }

        if (CasinoSessionActive && SecondsLeft <= 0f)
            FinishDay();
    }

    public void StartNewRun(string startingCasinoScene)
    {
        casinoScene = startingCasinoScene;
        Day = 1;
        dailyQuota = startingQuota;

        IsFinished = false;
        CasinoSessionActive = false;
        AwaitingPokerBoss = false;
        waitingForNextDay = false;
        enteringBossFight = false;
        startTimerOnCasinoLoad = true;

        ResetDailyPotions();
        permanentLuckMultiplier = 1f;

        GameData data = GameData.PobierzLubUtworz();
        data.Luck = StartingLuck;
        data.SaldoGracza = DailyMoney;

        Message = $"Your first daily quota is ${dailyQuota}.";
    }

    public void ContinueDay()
    {
        if (!waitingForNextDay || IsFinished)
            return;

        waitingForNextDay = false;
        startTimerOnCasinoLoad = true;
        SceneManager.LoadScene(casinoScene);
    }

    public void OnBalanceDepleted()
    {
        if (Day == 0 || IsFinished)
            return;

        EndRun("GAME OVER! You ran out of money.");
    }

    public bool TrySpend(int amount)
    {
        if (!CanUseCasino || amount <= 0)
            return false;

        GameData data = GameData.PobierzLubUtworz();

        if (data.SaldoGracza < amount)
            return false;

        data.SaldoGracza -= amount;
        return !IsFinished;
    }

    public void AddCash(int amount)
    {
        if (CasinoSessionActive && !IsFinished && amount > 0)
            GameData.PobierzLubUtworz().SaldoGracza += amount;
    }

    public int CreditWin(int payout)
    {
        if (!CasinoSessionActive || IsFinished || payout <= 0)
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
        luckyEndsAt = Math.Max(luckyEndsAt, Time.unscaledTimeAsDouble) + PotionDuration;
    }

    public void ActivateMoneyPotion()
    {
        moneyEndsAt = Math.Max(moneyEndsAt, Time.unscaledTimeAsDouble) + PotionDuration;
    }

    public void AddSmallLuckPotion()
    {
        GameData.PobierzLubUtworz().Luck += 10f;
    }

    public void DoublePermanentLuck()
    {
        permanentLuckMultiplier *= 2f;
    }

    public void EndRunFromDeathPotion()
    {
        if (CasinoSessionActive && !IsFinished)
            EndRun("GAME OVER! The Death potion ended your run.");
    }

    public bool TryMatchThirdSlotSymbol()
    {
        float luck = EffectiveLuck;

        if (luck >= 50f)
        {
            if (slotGuaranteeUsed)
                return false;

            slotGuaranteeUsed = true;
            return true;
        }

        if (luck >= 40f) return RollLuck(15);
        if (luck >= 30f) return RollLuck(10);
        if (luck >= 20f) return RollLuck(5);

        return false;
    }

    public bool TryRouletteReroll()
    {
        float luck = EffectiveLuck;

        if (luck >= 70f) return RollLuck(8);
        if (luck >= 60f) return RollLuck(6);
        if (luck >= 50f) return RollLuck(4);
        if (luck >= 40f) return RollLuck(3);
        if (luck >= 30f) return RollLuck(2);
        if (luck >= 20f) return RollLuck(1);

        return false;
    }

    public bool TryBlackjackReroll()
    {
        float luck = EffectiveLuck;

        if (luck >= 70f) return RollLuck(20);
        if (luck >= 60f) return RollLuck(15);
        if (luck >= 50f) return RollLuck(10);
        if (luck >= 40f) return RollLuck(8);
        if (luck >= 30f) return RollLuck(5);
        if (luck >= 20f) return RollLuck(3);

        return false;
    }

    private static bool RollLuck(int percent)
    {
        return UnityEngine.Random.Range(0, 100) < percent;
    }

    public void EnterBossFight()
    {
        if (!AwaitingPokerBoss || IsFinished || enteringBossFight ||
            SceneManager.GetActiveScene().name != casinoScene)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(pokerBossScene) ||
            !Application.CanStreamedLevelBeLoaded(pokerBossScene))
        {
            Debug.LogError($"Add '{pokerBossScene}' to the Build Profiles scene list.", this);
            return;
        }

        enteringBossFight = true;
        SceneManager.LoadScene(pokerBossScene);
    }

    public void ResolvePokerBoss(bool won)
    {
        if (!AwaitingPokerBoss || IsFinished ||
            SceneManager.GetActiveScene().name != pokerBossScene)
        {
            return;
        }

        AwaitingPokerBoss = false;
        enteringBossFight = false;

        if (!won)
        {
            EndRun("GAME OVER! You lost the dice duel.");
            return;
        }

        GameData.PobierzLubUtworz().SaldoGracza += bossReward;

        if (Day == FinalDay)
        {
            EndRun($"YOU WIN! Final boss defeated. Reward: ${bossReward}.");
            return;
        }

        Message = $"Boss defeated! Reward: ${bossReward}.";
        StartNextDay();
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
        GameData data = GameData.PobierzLubUtworz();

        if (data.SaldoGracza < dailyQuota)
        {
            int missing = dailyQuota - data.SaldoGracza;
            EndRun($"GAME OVER! You are ${missing} short of the daily quota.");
            return;
        }

        data.SaldoGracza -= dailyQuota;

        if (IsFinished)
            return;

        Message = $"Daily quota paid: ${dailyQuota}.";

        if (Day % BossInterval == 0)
        {
            AwaitingPokerBoss = true;
            Message += "\nThe casino is locked. Find the Boss Battle door.";
            SceneManager.LoadScene(casinoScene);
            return;
        }

        StartNextDay();
    }

    private void StartNextDay()
    {
        Day++;
        dailyQuota = Mathf.CeilToInt(dailyQuota * 1.5f);
        ResetDailyPotions();

        GameData.PobierzLubUtworz().SaldoGracza += DailyMoney;
        Message += $"\nDay {Day} starting money: +${DailyMoney}.";

        if (!string.IsNullOrWhiteSpace(newDayScene) &&
            Application.CanStreamedLevelBeLoaded(newDayScene))
        {
            waitingForNextDay = true;
            SceneManager.LoadScene(newDayScene);
            return;
        }

        startTimerOnCasinoLoad = true;
        SceneManager.LoadScene(casinoScene);
    }

    private void ResetDailyPotions()
    {
        soldBottles.Clear();
        luckyEndsAt = 0d;
        moneyEndsAt = 0d;
        slotGuaranteeUsed = false;
    }

    private void EndRun(string message)
    {
        IsFinished = true;
        CasinoSessionActive = false;
        AwaitingPokerBoss = false;
        waitingForNextDay = false;
        enteringBossFight = false;
        startTimerOnCasinoLoad = false;
        Message = message;

        SceneManager.LoadScene(casinoScene);
    }
}