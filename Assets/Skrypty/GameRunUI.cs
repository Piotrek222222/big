using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRunUI : MonoBehaviour
{
    [SerializeField] private string homeScene;
    [SerializeField] private string casinoScene = "save";
    [SerializeField] private TMP_Text statusText;

    private void LateUpdate()
    {
        GameRun run = GameRun.Instance;

        if (statusText == null || run == null || run.Day == 0)
            return;

        string status = $"Day {run.Day}  |  Cash: ${run.Cash}";

        if (run.CasinoSessionActive)
        {
            int seconds = Mathf.CeilToInt(run.SecondsLeft);
            status += $"  |  Time left: {seconds / 60:00}:{seconds % 60:00}";
        }

        if (!run.IsFinished)
            status += $"\nPocket money due: ${run.NextPayment}, day {run.NextDueDay}";

        statusText.text = $"{status}\n{run.Message}";
    }

    public void NewGame()
    {
        GameRun run = GameRun.Instance;

        if (run == null)
        {
            Debug.LogError("Dodaj GameRun do sceny MainMenu.", this);
            return;
        }

        if (!CanLoad(homeScene))
            return;

        run.StartNewRun(homeScene);
        SceneManager.LoadScene(homeScene);
    }

    public void GoToCasino()
    {
        GameRun run = GameRun.Instance;

        if (run != null && CanLoad(casinoScene) && run.StartCasinoSession())
            SceneManager.LoadScene(casinoScene);
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

        Debug.LogError($"Sprawdź scenę '{sceneName}' w Build Profiles → Scene List.", this);
        return false;
    }
}