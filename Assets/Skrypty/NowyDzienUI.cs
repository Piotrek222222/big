using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// UI for the "NowyDzien" intermission screen, shown after every in-game day.
/// Reads the current state from the persistent GameRun singleton and offers
/// two actions: continue to the casino (next day) or go back to the main menu.
/// </summary>
public class NowyDzienUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text statusText;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button menuButton;

    [Tooltip("Casino scene opened by Continue when no active run exists (editor testing only).")]
    [SerializeField] private string casinoSceneFallback = "save";

    private void Start() => Refresh();

    private void OnEnable() => Refresh();

    private void Refresh()
    {
        GameRun run = GameRun.Instance;
        bool hasRun = run != null && run.Day > 0;

        if (titleText != null)
        {
            if (!hasRun)
            {
                titleText.text = "NEW DAY";
            }
            else if (run.IsFinished)
            {
                titleText.text = run.Message.StartsWith("GAME OVER")
                    ? "GAME OVER"
                    : "YOU WIN!";
            }
            else
            {
                titleText.text = $"DAY {run.Day}";
            }
        }

        if (statusText != null)
        {
            if (!hasRun)
            {
                statusText.text =
                    "No active game run.\n\n" +
                    "Start a game from the Main Menu.\n" +
                    "(Continue opens the casino for a quick look around.)";
            }
            else
            {
                string status = $"Money: ${run.Money}";

                if (!run.IsFinished)
                {
                    status += $"\nAllowance due: ${run.NextPayment} by the end of day {run.NextDueDay}";
                }

                if (!string.IsNullOrEmpty(run.Message))
                {
                    status += $"\n\n{run.Message}";
                }

                statusText.text = status;
            }
        }

        // The run is over (win or game over): the only way forward is the menu.
        if (continueButton != null)
            continueButton.interactable = !hasRun || !run.IsFinished;
    }

    /// <summary>Starts the next casino day (or opens the casino for editor testing).</summary>
    public void Continue()
    {
        GameRun run = GameRun.Instance;

        if (run != null && run.Day > 0)
        {
            run.ContinueDay();
            return;
        }

        if (CanLoad(casinoSceneFallback))
            SceneManager.LoadScene(casinoSceneFallback);
    }

    public void GoToMenu()
    {
        if (CanLoad("MainMenu"))
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
