using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRunUI : MonoBehaviour
{
    [SerializeField] private string casinoScene = "save";
    [SerializeField] private TMP_Text statusText;

    private void LateUpdate()
    {
        if (statusText == null)
            return;

        GameRun run = GameRun.Instance;

        if (run == null || run.Day == 0)
        {
            statusText.text = "";
            return;
        }

        string status = $"Dzień {run.Day}  |  Gotówka: ${run.Cash}";

        if (run.CasinoSessionActive)
        {
            int seconds = Mathf.CeilToInt(run.SecondsLeft);
            status += $"  |  Czas: {seconds / 60:00}:{seconds % 60:00}";
        }

        if (!run.IsFinished)
            status += $"\nKieszonkowe: ${run.NextPayment} do końca dnia {run.NextDueDay}";

        if (!string.IsNullOrEmpty(run.Message))
            status += $"\n{run.Message}";

        statusText.text = status;
    }

    public void NewGame()
    {
        GameRun run = GameRun.Instance;

        if (run == null)
        {
            Debug.LogError("Dodaj GameRun do sceny MainMenu.", this);
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

        Debug.LogError($"Dodaj scenę '{sceneName}' do Build Profiles → Scene List.", this);
        return false;
    }
}