using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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

    public int stawkaZakladu = 10;
    public TypProp wybranyProp = TypProp.AnySeven;
    public TMP_Dropdown propDropdown;
    public TMP_Text saldoBossaText;
    public Zaklad aktualnyZaklad;
    public Zaklad aktualnyZakladBossa;

    public Button przycisk1;
    public Button przycisk2;
    public Button przycisk3;
    private int wybranyPrzycisk = -1;

    public Button rollButton;
    public int point;
    public List<int> historiaWynikow = new List<int>();
    private bool czyKliknietoRoll = false;
    private int postawionaKwotaGracza = 0;


    private void Start()
    {
        GameStart();
    }
    public void GameStart()
    {
        float mnoznikBossa = 1f;

        if (GameRun.Instance.Day == 6)
        {
            mnoznikBossa = 1.5f;
        }
        else if (GameRun.Instance.Day == 9)
        {
            mnoznikBossa = 2f;
        }

        SaldoBossa = Mathf.RoundToInt(GameRun.Instance.Money * mnoznikBossa);
        AktualizujSaldoBossa();
        UstawListeProp();
        StartCoroutine(KolejnoscTury());
    }

    private void AktualizujSaldoBossa()
    {
        if (saldoBossaText != null)
        {
            saldoBossaText.text = $"${SaldoBossa}";
        }
    }

    private IEnumerator KolejnoscTury()
    {
        while (GameData.PobierzLubUtworz().SaldoGracza > 0 && SaldoBossa > 0)
        {
            yield return StartCoroutine(CzekajNaPrzycisk());

            BossTurn();
            AktualizujSaldoBossa();

            yield return StartCoroutine(PrzebiegGry());

            if (GameData.PobierzLubUtworz().SaldoGracza <= 1 && postawionaKwotaGracza > 0)
            {
                GameData.PobierzLubUtworz().SaldoGracza = 0;
            }
        }

        if (SaldoBossa <= 0)
        {
            GameRun.Instance.ResolvePokerBoss(true);
        }
        else
        {
            GameRun.Instance.ResolvePokerBoss(false);
        }
    }

    private void UstawListeProp()
    {
        if (propDropdown == null) return;

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
        int realnaStawka = Mathf.Clamp(stawkaZakladu, 1, data.SaldoGracza);
        postawionaKwotaGracza = realnaStawka;

        if (data.SaldoGracza - realnaStawka <= 0)
        {
            data.SaldoGracza = 1;
        }
        else
        {
            data.SaldoGracza -= realnaStawka;
            postawionaKwotaGracza = 0;
        }

        return realnaStawka;
    }

    public void Pass()
    {
        int kwota = PobierzBezpiecznaStawke();
        aktualnyZaklad = new Zaklad
        {
            typ = TypZakladu.Pass,
            kwota = kwota
        };
    }

    public void DontPass()
    {
        int kwota = PobierzBezpiecznaStawke();
        aktualnyZaklad = new Zaklad
        {
            typ = TypZakladu.DontPass,
            kwota = kwota
        };
    }

    public void Prop()
    {
        int kwota = PobierzBezpiecznaStawke();
        aktualnyZaklad = new Zaklad
        {
            typ = TypZakladu.Prop,
            prop = wybranyProp,
            kwota = kwota
        };
    }

    public void BossTurn()
    {
        if (SaldoBossa <= 0)
            return;

        int saldoGracza = GameRun.Instance.Money;
        float stosunekSalda = (float)SaldoBossa / Mathf.Max(1, saldoGracza);

        int stawkaBossa;
        TypZakladu wybranyTyp;
        TypProp wybranyPropBossa = TypProp.AnySeven;

        if (stosunekSalda < 0.4f)
        {
            stawkaBossa = Mathf.Max(1, Mathf.RoundToInt(SaldoBossa * 0.4f));
        }
        else if (stosunekSalda > 1.5f)
        {
            stawkaBossa = Mathf.Max(1, Mathf.RoundToInt(SaldoBossa * 0.15f));
        }
        else
        {
            stawkaBossa = Mathf.RoundToInt(stawkaZakladu * Random.Range(0.8f, 1.3f));
        }

        stawkaBossa = Mathf.Clamp(stawkaBossa, 1, SaldoBossa);

        int los = Random.Range(0, 100);
        int szansaNaProp = stosunekSalda < 0.4f ? 25 : 10;

        if (los < szansaNaProp)
        {
            wybranyTyp = TypZakladu.Prop;

            if (stosunekSalda < 0.4f)
            {
                TypProp[] ryzykownePropy = { TypProp.Three, TypProp.Eleven, TypProp.Two, TypProp.Twelve };
                wybranyPropBossa = ryzykownePropy[Random.Range(0, ryzykownePropy.Length)];
            }
            else
            {
                wybranyPropBossa = Random.value < 0.65f ? TypProp.AnySeven : TypProp.AnyCraps;
            }
        }
        else
        {
            if (Random.value < 0.4f && aktualnyZaklad.typ != TypZakladu.Prop)
            {
                wybranyTyp = aktualnyZaklad.typ == TypZakladu.Pass
                    ? TypZakladu.DontPass
                    : TypZakladu.Pass;
            }
            else
            {
                wybranyTyp = Random.value < 0.5f ? TypZakladu.Pass : TypZakladu.DontPass;
            }
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

        TypZakladu wynikGlowny = TypZakladu.Pass;

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
                else if (score == 7)
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
                return historiaWynikow.Contains(2) || historiaWynikow.Contains(3) || historiaWynikow.Contains(12);
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
        {
            return zaklad.kwota * 2;
        }

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
        if (postawionaKwotaGracza > 0)
        {
            GameData.PobierzLubUtworz().SaldoGracza = wygrana;
            postawionaKwotaGracza = 0;
        }
        else
        {
            GameData.PobierzLubUtworz().SaldoGracza += wygrana;
        }
    }

    public void WynikBoss(TypZakladu wygrywajacyZaklad)
    {
        int wygrana = ObliczWyplate(aktualnyZakladBossa);
        SaldoBossa += wygrana;
        AktualizujSaldoBossa();
    }
}