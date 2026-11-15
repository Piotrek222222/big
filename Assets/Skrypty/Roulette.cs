using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RuletkaManager : MonoBehaviour
{
    public List<Button> squares = new List<Button>();
    public enum KolorPola { Zielony, Czerwony, Czarny }
    public enum Parzystosc { Brak, Even, Odd }

    [System.Serializable]
    public struct WynikRuletki
    {
        public string wyswietlanyNumer;
        public int wartoscLiczbowa;
        public KolorPola kolor;
        public int rzad;
        public int dwunastka;
        public Parzystosc parzystosc;
    }

    [System.Serializable]
    public struct Zaklad
    {
        public int indeksPola;
        public int kwota;
    }

    public int aktualnaStawka = 10;
    public int ostatniaWygrana;
    public Slider Bet_Size;
    public Text BetScore;
    public Text SaldoText;
    public TMP_Text WynikText;

    public WynikRuletki ostatniWynik;
    public List<WynikRuletki> historiaLosowan = new List<WynikRuletki>();
    public List<Zaklad> aktywneZaklady = new List<Zaklad>();

    private static readonly HashSet<int> czerwoneLiczby = new HashSet<int>
    {
        1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32
    };

    private int SaldoGracza
    {
        get => GameData.PobierzLubUtworz().SaldoGracza;
        set
        {
            GameData.PobierzLubUtworz().SaldoGracza = value;
            OdswiezSaldoUI();
        }
    }

    private float Luck => GameData.PobierzLubUtworz().Luck;

    private void Start()
    {
        if (WynikText != null)
        {
            WynikText.text = "";
        }

        if (Bet_Size != null)
        {
            aktualnaStawka = Mathf.RoundToInt(Bet_Size.value);
            Bet_Size.onValueChanged.AddListener(Zmiana);
            Zmiana(Bet_Size.value);
        }

        for (int i = 0; i < squares.Count; i++)
        {
            if (squares[i] == null) continue;
            int indeks = i;
            squares[i].onClick.AddListener(() => Bet(indeks));
        }

        // Upewniamy się, że na starcie wszystkie przyciski mają alpha = 0
        ResetujPrzezroczystoscWszystkichPol();
        OdswiezSaldoUI();
    }

    private void OdswiezSaldoUI()
    {
        if (SaldoText != null)
        {
            SaldoText.text = SaldoGracza.ToString();
        }
    }

    // NOWE: Zmienia kanał Alpha komponentu Image dla konkretnego przycisku (0f = 0%, 1f = 100%)
    private void UstawPrzezroczystoscPola(int indeksPola, float alpha)
    {
        if (indeksPola < 0 || indeksPola >= squares.Count || squares[indeksPola] == null)
            return;

        Image img = squares[indeksPola].GetComponent<Image>();
        if (img != null)
        {
            Color kolor = img.color;
            kolor.a = alpha;
            img.color = kolor;
        }
    }

    // NOWE: Ustawia alpha = 0 dla wszystkich przycisków na planszy
    private void ResetujPrzezroczystoscWszystkichPol()
    {
        for (int i = 0; i < squares.Count; i++)
        {
            UstawPrzezroczystoscPola(i, 0f);
        }
    }

    public void Zmiana(float wartosc)
    {
        aktualnaStawka = Mathf.RoundToInt(wartosc);

        if (BetScore != null)
        {
            BetScore.text = aktualnaStawka.ToString();
        }
    }

    public void Bet(int indeksPola)
    {
        if (SaldoGracza < aktualnaStawka || aktualnaStawka <= 0)
            return;

        SaldoGracza -= aktualnaStawka;

        // NOWE: Ustawia przezroczystość klikniętego przycisku na 100% (1f)
        UstawPrzezroczystoscPola(indeksPola, 1f);

        for (int i = 0; i < aktywneZaklady.Count; i++)
        {
            if (aktywneZaklady[i].indeksPola == indeksPola)
            {
                Zaklad zaktualizowany = aktywneZaklady[i];
                zaktualizowany.kwota += aktualnaStawka;
                aktywneZaklady[i] = zaktualizowany;
                return;
            }
        }

        aktywneZaklady.Add(new Zaklad
        {
            indeksPola = indeksPola,
            kwota = aktualnaStawka
        });
    }

    public void WyczyscZaklady()
    {
        int zwrot = 0;
        foreach (Zaklad zaklad in aktywneZaklady)
        {
            zwrot += zaklad.kwota;
        }
        aktywneZaklady.Clear();
        SaldoGracza += zwrot;

        // NOWE: Ustawia przezroczystość wszystkich pól z powrotem na 0% (0f)
        ResetujPrzezroczystoscWszystkichPol();
    }

    public void Losuj()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(3);
        }
        LosujIZapisz();
    }

    public WynikRuletki LosujIZapisz()
    {
        int wylosowana = Random.Range(0, 34);
        WynikRuletki wynik = AnalizujNumer(wylosowana);

        if (aktywneZaklady.Count > 0 && ObliczSumeWygranych(wynik) == 0)
        {
            if (Random.Range(0f, 100f) < Luck)
            {
                wylosowana = Random.Range(0, 34);
                wynik = AnalizujNumer(wylosowana);
            }
        }

        ostatniWynik = wynik;
        historiaLosowan.Add(wynik);

        if (WynikText != null)
        {
            string angielskiKolor = "";
            switch (wynik.kolor)
            {
                case KolorPola.Zielony:
                    angielskiKolor = "Green";
                    break;
                case KolorPola.Czerwony:
                    angielskiKolor = "Red";
                    break;
                case KolorPola.Czarny:
                    angielskiKolor = "Black";
                    break;
            }

            WynikText.text = $"{wynik.wyswietlanyNumer} {angielskiKolor}";
        }

        Rozliczenie();

        return wynik;
    }

    public void Rozliczenie()
    {
        int sumaWygranej = ObliczSumeWygranych(ostatniWynik);

        ostatniaWygrana = sumaWygranej;
        aktywneZaklady.Clear();
        SaldoGracza += sumaWygranej;

        // NOWE: Po rozliczeniu zakłady znikają ze stołu, więc zerujemy też podświetlenie
        ResetujPrzezroczystoscWszystkichPol();
    }

    private int ObliczSumeWygranych(WynikRuletki wynik)
    {
        int suma = 0;
        foreach (Zaklad zaklad in aktywneZaklady)
        {
            suma += ObliczWygranaZaZaklad(zaklad, wynik);
        }
        return suma;
    }

    private int ObliczWygranaZaZaklad(Zaklad zaklad, WynikRuletki wynik)
    {
        int idx = zaklad.indeksPola;
        int stawka = zaklad.kwota;

        if (idx >= 0 && idx <= 36)
            return (wynik.wartoscLiczbowa == idx) ? stawka * 36 : 0;

        if (idx >= 37 && idx <= 39)
        {
            int obstawionyRzad = idx - 36;
            return (wynik.rzad == obstawionyRzad) ? stawka * 3 : 0;
        }

        if (idx >= 40 && idx <= 42)
        {
            int obstawionaDwunastka = idx - 39;
            return (wynik.dwunastka == obstawionaDwunastka) ? stawka * 3 : 0;
        }

        if (idx == 43)
            return (wynik.kolor == KolorPola.Czerwony) ? stawka * 2 : 0;

        if (idx == 44)
            return (wynik.kolor == KolorPola.Czarny) ? stawka * 2 : 0;

        if (idx == 45)
            return (wynik.parzystosc == Parzystosc.Even) ? stawka * 2 : 0;

        if (idx == 46)
            return (wynik.parzystosc == Parzystosc.Odd) ? stawka * 2 : 0;

        return 0;
    }

    private WynikRuletki AnalizujNumer(int numer)
    {
        WynikRuletki wynik = new WynikRuletki
        {
            wartoscLiczbowa = numer,
            wyswietlanyNumer = numer.ToString()
        };

        if (numer == 0)
        {
            wynik.kolor = KolorPola.Zielony;
            wynik.rzad = 0;
            wynik.dwunastka = 0;
            wynik.parzystosc = Parzystosc.Brak;
            return wynik;
        }

        wynik.kolor = czerwoneLiczby.Contains(numer) ? KolorPola.Czerwony : KolorPola.Czarny;

        int reszta = numer % 3;
        if (reszta == 0) wynik.rzad = 1;
        else if (reszta == 2) wynik.rzad = 2;
        else wynik.rzad = 3;

        if (numer <= 12) wynik.dwunastka = 1;
        else if (numer <= 24) wynik.dwunastka = 2;
        else wynik.dwunastka = 3;

        wynik.parzystosc = (numer % 2 == 0) ? Parzystosc.Even : Parzystosc.Odd;

        return wynik;
    }
}