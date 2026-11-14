using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRun : MonoBehaviour
{
    public static GameRun Instance { get; private set; }

    private const float SessionDuration = 180f;
    private static readonly int[] Payments = { 100, 175, 275 };

    private string homeScene;
    private double sessionEndsAt;

    public int Day { get; private set; }
    public int Cash { get; private set; }
    public bool IsFinished { get; private set; }
    public bool CasinoSessionActive { get; private set; }
    public string Message { get; private set; } = "";

    public int NextDueDay => Day == 0 || IsFinished ? 0 : ((Day - 1) / 3 + 1) * 3;
    public int NextPayment => Day == 0 || IsFinished ? 0 : Payments[(Day - 1) / 3];

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
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (CasinoSessionActive && SecondsLeft <= 0f)
            FinishCasinoSession();
    }

    public void StartNewRun(string startingHomeScene)
    {
        homeScene = startingHomeScene;
        Day = 1;
        Cash = 60;
        IsFinished = false;
        CasinoSessionActive = false;
        Message = "Pay up at the end of the third day!";
    }

    public bool StartCasinoSession()
    {
        if (Day == 0 || IsFinished)
            return false;

  
        if (CasinoSessionActive)
            return SecondsLeft > 0f;

        sessionEndsAt = Time.unscaledTimeAsDouble + SessionDuration;
        CasinoSessionActive = true;
        Message = "";
        return true;
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

    private void FinishCasinoSession()
    {
        CasinoSessionActive = false;
        Message = "Times up buddy!";

        if (Day == NextDueDay)
        {
            int payment = NextPayment;

            if (Cash < payment)
            {
                IsFinished = true;
                Message = $"You are broke! You needed ${payment - Cash} for your childrens pocket money.";
            }
            else
            {
                Cash -= payment;
                IsFinished = Day == Payments.Length * 3;
                Message = IsFinished
                    ? "You won all victories are transfered to you"
                    : $"Paid ${payment} of pocket money.";
            }
        }

        if (!IsFinished)
            Day++;

        SceneManager.LoadScene(homeScene);
    }
}