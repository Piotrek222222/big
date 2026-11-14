using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRun : MonoBehaviour
{
    public static GameRun Instance { get; private set; }

    [SerializeField, Min(1f)] private float sessionDuration = 180f;

    private static readonly int[] Payments = { 100, 175, 275 };
    private const int DaysBetweenPayments = 3;

    private string casinoScene;
    private double sessionEndsAt;
    private bool startTimerOnCasinoLoad;

    public int Day { get; private set; }
    public int Cash { get; private set; }
    public bool IsFinished { get; private set; }
    public bool CasinoSessionActive { get; private set; }
    public string Message { get; private set; } = "";

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
        Cash = 60;
        IsFinished = false;
        CasinoSessionActive = false;
        startTimerOnCasinoLoad = true;
        Message = "Pay up at the end of day 3.";
    }

    public bool TrySpend(int amount)
    {
        if (!CasinoSessionActive || SecondsLeft <= 0f ||
            amount <= 0 || Cash < amount)
        {
            return false;
        }

        Cash -= amount;
        return true;
    }

    public void AddCash(int amount)
    {
        if (CasinoSessionActive && amount > 0)
            Cash += amount;
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
        Message = "Times up buddy";

        if (Day == NextDueDay)
        {
            int payment = NextPayment;

            if (Cash < payment)
            {
                IsFinished = true;
                Message = $"You lost ${payment - Cash} due for pocket money.";
            }
            else
            {
                Cash -= payment;
                IsFinished = Day == Payments.Length * DaysBetweenPayments;

                Message = IsFinished
                    ? "You Won!."
                    : $"Paid ${payment} of pocket money.";
            }
        }

        if (!IsFinished)
        {
            Day++;
            startTimerOnCasinoLoad = true;
        }

        SceneManager.LoadScene(casinoScene);
    }
}