using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRun : MonoBehaviour
{
    public static GameRun Instance { get; private set; }

    [SerializeField, Min(1f)] private float sessionDuration = 180f;

    private const int StartingMoney = 60;
    private const int DaysBetweenPayments = 3;
    private static readonly int[] Payments = { 100, 175, 275 };

    // Intermission screen shown after every in-game day.
    public const string NowyDzienScene = "NowyDzien";

    private string casinoScene;
    private double sessionEndsAt;
    private bool startTimerOnCasinoLoad;

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
