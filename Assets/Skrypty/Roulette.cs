using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public int saldoGracza = 1000;
    public int aktualnaStawka = 10;
    public int ostatniaWygrana;
    public Slider Bet_Size;
    public WynikRuletki ostatniWynik;
    public List<WynikRuletki> historiaLosowan = new List<WynikRuletki>();
    public List<Zaklad> aktywneZaklady = new List<Zaklad>();

    private static readonly HashSet<int> czerwoneLiczby = new HashSet<int>
    {
        1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36
    };

    private void Start()
    {
        if (Bet_Size != null)
        {
            aktualnaStawka = Mathf.RoundToInt(Bet_Size.value);
            Bet_Size.onValueChanged.AddListener(Zmiana);
        }

        for (int i = 0; i < squares.Count; i++)
        {
            if (squares[i] == null) continue;
            int indeks = i;
            squares[i].onClick.AddListener(() => Bet(indeks));
        }
    }

    public void Zmiana(float wartosc)
    {
        aktualnaStawka = Mathf.RoundToInt(wartosc);
    }

    public void Bet(int indeksPola)
    {
        if (saldoGracza < aktualnaStawka || aktualnaStawka <= 0)
            return;

        saldoGracza -= aktualnaStawka;

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
        foreach (Zaklad zaklad in aktywneZaklady)
        {
            saldoGracza += zaklad.kwota;
        }
        aktywneZaklady.Clear();
    }

    public void Losuj()
    {
        LosujIZapisz();
    }

    public WynikRuletki LosujIZapisz()
    {
        int wylosowana = Random.Range(0, 37);
        WynikRuletki wynik = AnalizujNumer(wylosowana);

        ostatniWynik = wynik;
        historiaLosowan.Add(wynik);

        Rozliczenie();

        return wynik;
    }

    public void Rozliczenie()
    {
        int sumaWygranej = 0;

        foreach (Zaklad zaklad in aktywneZaklady)
        {
            sumaWygranej += ObliczWygranaZaZaklad(zaklad, ostatniWynik);
        }

        ostatniaWygrana = sumaWygranej;
        saldoGracza += sumaWygranej;
        aktywneZaklady.Clear();
    }

    private int ObliczWygranaZaZaklad(Zaklad zaklad, WynikRuletki wynik)
    {
        int idx = zaklad.indeksPola;
        int stawka = zaklad.kwota;

        if (idx >= 0 && idx <= 36)
        {
            return (wynik.wartoscLiczbowa == idx) ? stawka * 36 : 0;
        }

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
        {
            return (wynik.kolor == KolorPola.Czerwony) ? stawka * 2 : 0;
        }

        if (idx == 44)
        {
            return (wynik.kolor == KolorPola.Czarny) ? stawka * 2 : 0;
        }

        if (idx == 45)
        {
            return (wynik.parzystosc == Parzystosc.Even) ? stawka * 2 : 0;
        }

        if (idx == 46)
        {
            return (wynik.parzystosc == Parzystosc.Odd) ? stawka * 2 : 0;
        }

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