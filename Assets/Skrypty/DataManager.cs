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
            saldoGracza = value;
            PlayerPrefs.SetInt("Global_Saldo", saldoGracza);
            PlayerPrefs.Save();
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

    // Automatycznie tworzy GameData, jeśli odpalisz scenę z ruletką bezpośrednio w edytorze
    public static GameData PobierzLubUtworz()
    {
        if (Instance == null)
        {
            GameData istniejacy = FindObjectOfType<GameData>();
            if (istniejacy != null)
            {
                Instance = istniejacy;
            }
            else
            {
                GameObject go = new GameObject("GameData (Auto)");
                Instance = go.AddComponent<GameData>();
            }
        }
        return Instance;
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

    public void WczytajDane()
    {
        saldoGracza = PlayerPrefs.GetInt("Global_Saldo", domyslneSaldo);
        luck = PlayerPrefs.GetFloat("Global_Luck", domyslnyLuck);
    }

    public void ResetujDane()
    {
        PlayerPrefs.DeleteKey("Global_Saldo");
        PlayerPrefs.DeleteKey("Global_Luck");
        saldoGracza = domyslneSaldo;
        luck = domyslnyLuck;
        PlayerPrefs.Save();
    }
}