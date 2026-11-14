using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRunUI : MonoBehaviour
{
    [SerializeField] private string casinoScene = "save";
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text timerText;

    private void LateUpdate()
    {
        GameRun run = GameRun.Instance;
        bool hasRun = run != null && run.Day > 0;

        if (statusText != null)
        {
            string status = "";

            if (hasRun)
            {
                status = $"Day {run.Day} | Money: ${run.Money}\n" +
                         $"Luck: {run.EffectiveLuck:0.##}";

                if (!run.IsFinished)
                    status += $"\nAllowance due: ${run.NextPayment} by the end of day {run.NextDueDay}";

                if (!string.IsNullOrEmpty(run.Message))
                    status += $"\n{run.Message}";
            }

            statusText.text = status;
        }

        if (timerText != null)
        {
            if (hasRun && run.CasinoSessionActive)
            {
                int seconds = Mathf.CeilToInt(run.SecondsLeft);
                timerText.text = $"{seconds / 60:00}:{seconds % 60:00}";
            }
            else
            {
                timerText.text = "";
            }
        }
    }

    public void NewGame()
    {
        GameRun run = GameRun.Instance;

        if (run == null)
        {
            Debug.LogError("Add a GameRun object to MainMenu.", this);
            return;
        }

        if (!CanLoad(casinoScene))
            return;

        run.StartNewRun(casinoScene);
        SceneManager.LoadScene(casinoScene);
    }

    public void GoToCasino()
    {
        GameRun run = GameRun.Instance;

        if (run != null && run.CasinoSessionActive &&
            run.SecondsLeft > 0f && CanLoad(casinoScene))
        {
            SceneManager.LoadScene(casinoScene);
        }
    }

    public void GoToMenu()
    {
        GameRun run = GameRun.Instance;

        if ((run == null || !run.CasinoSessionActive) && CanLoad("MainMenu"))
            SceneManager.LoadScene("MainMenu");
    }

    private bool CanLoad(string sceneName)
    {
        if (!string.IsNullOrWhiteSpace(sceneName) &&
            Application.CanStreamedLevelBeLoaded(sceneName))
        {
            return true;
        }

        Debug.LogError($"Add scene '{sceneName}' to the Build Profiles scene list.", this);
        return false;
    }
}