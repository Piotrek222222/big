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

    public void GameStart()
    {
        SaldoBossa = GameRun.Instance.Money;
        UstawListeProp();
        StartCoroutine(CzekajNaPrzycisk());
        BossTurn();
        Game();
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

    public void Pass()
    {
        GameRun.Instance.TrySpend(stawkaZakladu);
        aktualnyZaklad = new Zaklad
        {
            typ = TypZakladu.Pass,
            kwota = stawkaZakladu
        };
    }

    public void DontPass()
    {
        GameRun.Instance.TrySpend(stawkaZakladu);
        aktualnyZaklad = new Zaklad
        {
            typ = TypZakladu.DontPass,
            kwota = stawkaZakladu
        };
    }

    public void Prop()
    {
        GameRun.Instance.TrySpend(stawkaZakladu);
        aktualnyZaklad = new Zaklad
        {
            typ = TypZakladu.Prop,
            prop = wybranyProp,
            kwota = stawkaZakladu
        };
    }

    public void BossTurn()
    {

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
        GameData.PobierzLubUtworz().SaldoGracza += wygrana;
    }

    public void WynikBoss(TypZakladu wygrywajacyZaklad)
    {
        int wygrana = ObliczWyplate(aktualnyZakladBossa);
        SaldoBossa += wygrana;
    }
}