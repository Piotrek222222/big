using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dices : MonoBehaviour
{
    public enum TypZakladu
    {
        Pass,
        DontPass,
        Prop
    }

    public enum TypProp
    {
        AnySeven,
        AnyCraps,
        Three,
        Eleven,
        Two,
        Twelve
    }

    [System.Serializable]
    public struct Zaklad
    {
        public TypZakladu typ;
        public TypProp prop;
        public int kwota;
    }

    public struct ZakladBossa
    {
        public TypZakladu typ;
        public TypProp prop;
        public int kwota;
    }

    public Image Dice1;
    public Image Dice2;
    public int dice1S;
    public int dice2S;
    public int score;
    public int passscore;
    public int SaldoBossa;
    public List<Sprite> walls = new List<Sprite>();

    public int stawkaZakladu;
    public TypProp wybranyProp = TypProp.AnySeven;
    public TMP_Dropdown propDropdown;
    public TMP_Text saldoBossaText;
    public TMP_Text saldoGraczaText;
    public Zaklad aktualnyZaklad;
    public Zaklad aktualnyZakladBossa;

    public Button przycisk1;
    public Button przycisk2;
    public Button przycisk3;
    public Button rollButton;

    public int point;
    public List<int> historiaWynikow = new List<int>();

    private static readonly TypProp[] DostepnePropy =
        (TypProp[])System.Enum.GetValues(typeof(TypProp));

    private int wybranyPrzycisk = -1;
    private bool czyKliknietoRoll;
    private int postawionaKwotaGracza;
    private int saldoGraczaPrzedZakladem;

    private void Start()
    {
        GameStart();
    }

    public void GameStart()
    {
        GameRun run = GameRun.Instance;

        if (run == null)
        {
            Debug.LogError("Uruchom pojedynek z pokoju bossa.", this);
            return;
        }

        int saldoStartoweGracza = run.Money;
        stawkaZakladu = Mathf.Max(1, saldoStartoweGracza / 5);

        float mnoznikSaldaBossa = 1f;

        if (run.Day == 6)
            mnoznikSaldaBossa = 1.5f;
        else if (run.Day == 9)
            mnoznikSaldaBossa = 2f;

        SaldoBossa = Mathf.RoundToInt(saldoStartoweGracza * mnoznikSaldaBossa);

        AktualizujSaldoGracza();
        AktualizujSaldoBossa();
        UstawListeProp();
        StartCoroutine(KolejnoscTury());
    }

    private void AktualizujSaldoGracza()
    {
        if (saldoGraczaText == null)
            return;

        // Podczas all-in zapisane $1 pozwala dokończyć rundę.
        int saldo = postawionaKwotaGracza > 0
            ? 0
            : GameData.PobierzLubUtworz().SaldoGracza;

        int nastepnaStawka = SaldoBossa > 0
            ? Mathf.Min(stawkaZakladu, saldo)
            : 0;

        saldoGraczaText.text = $"${saldo}\nNext stake: ${nastepnaStawka}";
    }

    private void AktualizujSaldoBossa()
    {
        if (saldoBossaText == null)
            return;

        int saldoGracza = postawionaKwotaGracza > 0
            ? 0
            : GameData.PobierzLubUtworz().SaldoGracza;

        int nastepnaStawka = saldoGracza > 0
            ? ObliczStawkeBossa(saldoGracza)
            : 0;

        saldoBossaText.text = $"${SaldoBossa}\nNext stake: ${nastepnaStawka}";
    }

    private int ObliczStawkeBossa(int saldoGracza)
    {
        if (SaldoBossa <= 0)
            return 0;

        float stosunekSalda = (float)SaldoBossa / Mathf.Max(1, saldoGracza);
        float mnoznikStawki = 1f;

        if (stosunekSalda < 0.4f)
            mnoznikStawki = 2f;
        else if (stosunekSalda < 0.8f)
            mnoznikStawki = 1.5f;
        else if (stosunekSalda > 2f)
            mnoznikStawki = 0.5f;
        else if (stosunekSalda > 1.25f)
            mnoznikStawki = 0.75f;

        return Mathf.Clamp(
            Mathf.RoundToInt(stawkaZakladu * mnoznikStawki),
            1,
            SaldoBossa
        );
    }

    private IEnumerator KolejnoscTury()
    {
        GameData data = GameData.PobierzLubUtworz();

        while (data.SaldoGracza > 0 && SaldoBossa > 0)
        {
            yield return StartCoroutine(CzekajNaPrzycisk());

            BossTurn();
            AktualizujSaldoBossa();

            yield return StartCoroutine(PrzebiegGry());

            if (data.SaldoGracza <= 1 && postawionaKwotaGracza > 0)
            {
                postawionaKwotaGracza = 0;
                data.SaldoGracza = 0;
            }

            AktualizujSaldoGracza();
            AktualizujSaldoBossa();
        }

        GameRun.Instance.ResolvePokerBoss(SaldoBossa <= 0);
    }

    private void UstawListeProp()
    {
        if (propDropdown == null)
            return;

        propDropdown.ClearOptions();
        propDropdown.AddOptions(new List<string>
        {
            "Any Seven (7) - 4:1",
            "Any Craps (2, 3, 12) - 7:1",
            "Three (3) - 15:1",
            "Eleven (11) - 15:1",
            "Two (2) - 30:1",
            "Twelve (12) - 30:1"
        });

        propDropdown.onValueChanged.RemoveAllListeners();
        propDropdown.onValueChanged.AddListener(index => wybranyProp = (TypProp)index);
        wybranyProp = (TypProp)propDropdown.value;
    }

    private IEnumerator CzekajNaPrzycisk()
    {
        wybranyPrzycisk = -1;

        przycisk1.onClick.AddListener(() => wybranyPrzycisk = 1);
        przycisk2.onClick.AddListener(() => wybranyPrzycisk = 2);
        przycisk3.onClick.AddListener(() => wybranyPrzycisk = 3);

        yield return new WaitUntil(() => wybranyPrzycisk != -1);

        przycisk1.onClick.RemoveAllListeners();
        przycisk2.onClick.RemoveAllListeners();
        przycisk3.onClick.RemoveAllListeners();

        switch (wybranyPrzycisk)
        {
            case 1:
                Pass();
                break;

            case 2:
                DontPass();
                break;

            case 3:
                Prop();
                break;
        }
    }

    private int PobierzBezpiecznaStawke()
    {
        GameData data = GameData.PobierzLubUtworz();
        saldoGraczaPrzedZakladem = data.SaldoGracza;

        int realnaStawka = Mathf.Min(stawkaZakladu, saldoGraczaPrzedZakladem);
        postawionaKwotaGracza = 0;

        if (realnaStawka <= 0)
            return 0;

        if (realnaStawka == saldoGraczaPrzedZakladem)
        {
            postawionaKwotaGracza = realnaStawka;
            data.SaldoGracza = 1;
        }
        else
        {
            data.SaldoGracza -= realnaStawka;
        }

        AktualizujSaldoGracza();
        return realnaStawka;
    }

    public void Pass()
    {
        aktualnyZaklad = new Zaklad
        {
            typ = TypZakladu.Pass,
            kwota = PobierzBezpiecznaStawke()
        };
    }

    public void DontPass()
    {
        aktualnyZaklad = new Zaklad
        {
            typ = TypZakladu.DontPass,
            kwota = PobierzBezpiecznaStawke()
        };
    }

    public void Prop()
    {
        aktualnyZaklad = new Zaklad
        {
            typ = TypZakladu.Prop,
            prop = wybranyProp,
            kwota = PobierzBezpiecznaStawke()
        };
    }

    public void BossTurn()
    {
        if (SaldoBossa <= 0)
            return;

        float stosunekSalda =
            (float)SaldoBossa / Mathf.Max(1, saldoGraczaPrzedZakladem);

        int stawkaBossa = ObliczStawkeBossa(saldoGraczaPrzedZakladem);
        int szansaNaProp = stosunekSalda < 0.4f ? 25 : 10;

        TypZakladu wybranyTyp;
        TypProp wybranyPropBossa = TypProp.AnySeven;

        if (Random.Range(0, 100) < szansaNaProp)
        {
            wybranyTyp = TypZakladu.Prop;
            int indeks = Random.Range(0, DostepnePropy.Length);
            wybranyPropBossa = DostepnePropy[indeks];
        }
        else if (aktualnyZaklad.typ != TypZakladu.Prop && Random.value < 0.4f)
        {
            wybranyTyp = aktualnyZaklad.typ == TypZakladu.Pass
                ? TypZakladu.DontPass
                : TypZakladu.Pass;
        }
        else
        {
            wybranyTyp = Random.value < 0.5f
                ? TypZakladu.Pass
                : TypZakladu.DontPass;
        }

        SaldoBossa -= stawkaBossa;

        aktualnyZakladBossa = new Zaklad
        {
            typ = wybranyTyp,
            prop = wybranyPropBossa,
            kwota = stawkaBossa
        };
    }

    public void Roll()
    {
        dice1S = Random.Range(1, 7);
        dice2S = Random.Range(1, 7);
        score = dice1S + dice2S;

        Dice1.sprite = walls[dice1S - 1];
        Dice2.sprite = walls[dice2S - 1];

        czyKliknietoRoll = true;
    }

    public void Game()
    {
        StartCoroutine(PrzebiegGry());
    }

    private IEnumerator PrzebiegGry()
    {
        historiaWynikow.Clear();
        point = 0;

        rollButton.onClick.RemoveAllListeners();
        rollButton.onClick.AddListener(Roll);

        czyKliknietoRoll = false;
        yield return new WaitUntil(() => czyKliknietoRoll);
        historiaWynikow.Add(score);

        TypZakladu wynikGlowny;

        if (score == 7 || score == 11)
        {
            wynikGlowny = TypZakladu.Pass;
        }
        else if (score == 2 || score == 3 || score == 12)
        {
            wynikGlowny = TypZakladu.DontPass;
        }
        else
        {
            point = score;

            while (true)
            {
                czyKliknietoRoll = false;
                yield return new WaitUntil(() => czyKliknietoRoll);
                historiaWynikow.Add(score);

                if (score == point)
                {
                    wynikGlowny = TypZakladu.Pass;
                    break;
                }

                if (score == 7)
                {
                    wynikGlowny = TypZakladu.DontPass;
                    break;
                }
            }
        }

        rollButton.onClick.RemoveAllListeners();

        if (aktualnyZaklad.typ == TypZakladu.Prop)
        {
            if (CzyTrafionoProp(aktualnyZaklad.prop))
                Wynik(TypZakladu.Prop);
        }
        else if (aktualnyZaklad.typ == wynikGlowny)
        {
            Wynik(wynikGlowny);
        }

        if (aktualnyZakladBossa.typ == TypZakladu.Prop)
        {
            if (CzyTrafionoProp(aktualnyZakladBossa.prop))
                WynikBoss(TypZakladu.Prop);
        }
        else if (aktualnyZakladBossa.typ == wynikGlowny)
        {
            WynikBoss(wynikGlowny);
        }
    }

    private bool CzyTrafionoProp(TypProp rodzajProp)
    {
        switch (rodzajProp)
        {
            case TypProp.AnySeven:
                return historiaWynikow.Contains(7);

            case TypProp.AnyCraps:
                return historiaWynikow.Contains(2) ||
                       historiaWynikow.Contains(3) ||
                       historiaWynikow.Contains(12);

            case TypProp.Three:
                return historiaWynikow.Contains(3);

            case TypProp.Eleven:
                return historiaWynikow.Contains(11);

            case TypProp.Two:
                return historiaWynikow.Contains(2);

            case TypProp.Twelve:
                return historiaWynikow.Contains(12);

            default:
                return false;
        }
    }

    private int ObliczWyplate(Zaklad zaklad)
    {
        if (zaklad.typ == TypZakladu.Pass || zaklad.typ == TypZakladu.DontPass)
            return zaklad.kwota * 2;

        int mnoznik = 0;

        switch (zaklad.prop)
        {
            case TypProp.AnySeven:
                mnoznik = 4;
                break;

            case TypProp.AnyCraps:
                mnoznik = 7;
                break;

            case TypProp.Three:
            case TypProp.Eleven:
                mnoznik = 15;
                break;

            case TypProp.Two:
            case TypProp.Twelve:
                mnoznik = 30;
                break;
        }

        return zaklad.kwota * mnoznik + zaklad.kwota;
    }

    public void Wynik(TypZakladu wygrywajacyZaklad)
    {
        int wygrana = ObliczWyplate(aktualnyZaklad);
        GameData data = GameData.PobierzLubUtworz();

        if (postawionaKwotaGracza > 0)
        {
            postawionaKwotaGracza = 0;
            data.SaldoGracza = wygrana;
        }
        else
        {
            data.SaldoGracza += wygrana;
        }

        SaldoBossa = Mathf.Max(0, SaldoBossa - wygrana);

        AktualizujSaldoGracza();
        AktualizujSaldoBossa();
    }

    public void WynikBoss(TypZakladu wygrywajacyZaklad)
    {
        int wygrana = ObliczWyplate(aktualnyZakladBossa);
        SaldoBossa += wygrana;

        GameData data = GameData.PobierzLubUtworz();
        data.SaldoGracza = Mathf.Max(0, data.SaldoGracza - wygrana);

        AktualizujSaldoBossa();
        AktualizujSaldoGracza();
    }
}