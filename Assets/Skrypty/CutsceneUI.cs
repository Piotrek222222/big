using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// UI for the opening cutscene shown after clicking PLAY in the main menu.
/// Displays placeholder story text and starts the casino run when the player
/// presses Continue (or any key).
/// </summary>
public class CutsceneUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text bodyText;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;

    [Tooltip("Story text shown during the cutscene. TODO: fill in the real text.")]
    [SerializeField, TextArea] private string cutsceneText =
        "TODO: Opening cutscene text.\n\n" +
        "Edit the Cutscene Text field on the CutsceneUI component to replace this placeholder.";

    [Tooltip("Casino scene loaded after the cutscene ends.")]
    [SerializeField] private string casinoScene = "save";

    private bool started;
    private float skipInputReadyAt;

    private void Start()
    {
        if (bodyText != null && !string.IsNullOrWhiteSpace(cutsceneText))
            bodyText.text = cutsceneText;

        // Ignore skip input briefly so a double-click on PLAY doesn't skip the cutscene.
        skipInputReadyAt = Time.unscaledTime + 0.5f;
    }

    private void Update()
    {
        // Allow skipping the cutscene with any key press.
        if (Time.unscaledTime >= skipInputReadyAt && Input.anyKeyDown)
            Continue();
    }

    /// <summary>Ends the cutscene and starts the casino run.</summary>
    public void Continue()
    {
        if (started)
            return;

        if (!CanLoad(casinoScene))
            return;

        GameRun run = GameRun.Instance;

        if (run != null)
        {
            run.StartNewRun(casinoScene);
        }
        else
        {
            // Editor testing: open the casino without an active run.
            Debug.LogWarning("GameRun not found - opening the casino without a run.", this);
        }

        started = true;
        SceneManager.LoadScene(casinoScene);
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
