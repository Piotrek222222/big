using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    [Header("Zmienne globalne")]
    [SerializeField] private int domyslneSaldo = 10000;
    [SerializeField] private float domyslnyLuck = 0f;
    [SerializeField] private int saldoGracza = 10000;
    [SerializeField] private float luck = 0f;

    public int SaldoGracza
    {
        get => saldoGracza;
        set
        {
            saldoGracza = Mathf.Max(0, value);
            PlayerPrefs.SetInt("Global_Saldo", saldoGracza);
            PlayerPrefs.Save();

            GameRun run = GameRun.Instance;

            if (saldoGracza == 0 && run != null)
                run.OnBalanceDepleted();
        }
    }

    public float Luck
    {
        get => luck;
        set
        {
            luck = value;
            PlayerPrefs.SetFloat("Global_Luck", luck);
            PlayerPrefs.Save();
        }
    }

    public static GameData PobierzLubUtworz()
    {
        if (Instance != null)
            return Instance;

        GameData existing = FindFirstObjectByType<GameData>();

        if (existing != null)
        {
            Instance = existing;
            return existing;
        }

        return new GameObject("GameData (Auto)").AddComponent<GameData>();
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
        WczytajDane();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void WczytajDane()
    {
        saldoGracza = Mathf.Max(0, PlayerPrefs.GetInt("Global_Saldo", domyslneSaldo));
        luck = PlayerPrefs.GetFloat("Global_Luck", domyslnyLuck);
    }

    public void ResetujDane()
    {
        PlayerPrefs.DeleteKey("Global_Saldo");
        PlayerPrefs.DeleteKey("Global_Luck");

        saldoGracza = Mathf.Max(0, domyslneSaldo);
        luck = domyslnyLuck;

        PlayerPrefs.Save();
    }
}