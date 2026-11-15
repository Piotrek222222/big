using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRunUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text dayCashText;

    private const string CasinoSceneName = "save";
    private const string CutsceneSceneName = "Cutscene";

    private void LateUpdate()
    {
        // Canvas w MainMenu może używać tego skryptu tylko do obsługi przycisku.
        if (timerText == null && dayCashText == null)
            return;

        GameRun run = GameRun.Instance;

        if (run == null || run.Day == 0)
        {
            SetText(timerText, "--:--");
            SetText(dayCashText, run == null ? "GameRun missing" : "Run not started");
            return;
        }

        int seconds = Mathf.CeilToInt(run.SecondsLeft);
        SetText(timerText, $"{seconds / 60:00}:{seconds % 60:00}");

        string status = $"Day {run.Day} | Cash ${run.Money} | Luck {run.EffectiveLuck:0.##}";

        if (run.NextPayment > 0)
            status += $"\nQuota ${run.NextPayment}";

        if ((run.IsFinished || run.AwaitingPokerBoss) &&
            !string.IsNullOrEmpty(run.Message))
        {
            status += $"\n{run.Message}";
        }

        SetText(dayCashText, status);
    }

    public void NewGame()
    {
        // The opening cutscene leads into the casino run.
        if (Application.CanStreamedLevelBeLoaded(CutsceneSceneName))
        {
            SceneManager.LoadScene(CutsceneSceneName);
            return;
        }

        // Fallback for when the cutscene scene is missing from the build.
        StartCasinoRun();
    }

    public void StartCasinoRun()
    {
        GameRun run = GameRun.Instance;

        if (run == null)
        {
            Debug.LogError("GameRun is missing from MainMenu.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(CasinoSceneName))
        {
            Debug.LogError("Enable the 'save' scene in the build scene list.", this);
            return;
        }

        run.StartNewRun(CasinoSceneName);
        SceneManager.LoadScene(CasinoSceneName);
    }

    public void ContinueDay()
    {
        GameRun run = GameRun.Instance;

        if (run != null)
            run.ContinueDay();
    }

    private static void SetText(TMP_Text label, string value)
    {
        if (label != null && label.text != value)
            label.text = value;
    }
}
