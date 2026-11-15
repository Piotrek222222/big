using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackJack : MonoBehaviour
{
    public Cards Cards;
    public Image PlayerCard1;
    public Image DealerCard1;
    public int aktualnaStawka = 0;
    public bool isGameActive = false;
    public int playerScore = 0;
    public int dealerScore = 0;
    public int playerAces = 0;
    public int dealerAces = 0;
    public string lastDrawnCardName;
    public List<int> usedCardIndices = new List<int>();
    public float cardOffsetX = 45f;
    public List<Image> spawnedPlayerCards = new List<Image>();
    public List<Image> spawnedDealerCards = new List<Image>();
    public Text Player_Score;
    public Text Dealer_Score;
    public bool isHit;
    public Slider Bet_Size;
    public Text BetScore;
    public Text SaldoText;
    public Text ResultText;

    private bool luckRerollUsed;

    private int SaldoGracza
    {
        get => GameData.PobierzLubUtworz().SaldoGracza;
        set
        {
            GameData.PobierzLubUtworz().SaldoGracza = value;
            OdswiezSaldoUI();
        }
    }

    private void Awake()
    {
        Cards = GetComponent<Cards>();
        PlayerCard1.gameObject.SetActive(false);
        DealerCard1.gameObject.SetActive(false);

        if (Bet_Size != null)
        {
            aktualnaStawka = Mathf.RoundToInt(Bet_Size.value);
            Bet_Size.onValueChanged.AddListener(Zmiana);
            Zmiana(Bet_Size.value);
        }

        OdswiezSaldoUI();
    }

    private void OdswiezSaldoUI()
    {
        if (SaldoText != null)
            SaldoText.text = SaldoGracza.ToString();
    }

    private void UstawWynikUI(string komunikat)
    {
        if (ResultText != null)
            ResultText.text = komunikat;
    }

    public void Bet()
    {
        if (isGameActive)
            return;

        if (Bet_Size != null)
            aktualnaStawka = Mathf.RoundToInt(Bet_Size.value);

        if (aktualnaStawka <= 0 || SaldoGracza < aktualnaStawka)
            return;

        SaldoGracza -= aktualnaStawka;
        UstawWynikUI("");

        PlayerCard1.gameObject.SetActive(true);
        DealerCard1.gameObject.SetActive(true);

        isHit = false;
        luckRerollUsed = false;
        usedCardIndices.Clear();
        playerScore = 0;
        dealerScore = 0;
        playerAces = 0;
        dealerAces = 0;

        if (Player_Score != null)
            Player_Score.text = playerScore.ToString();

        if (Dealer_Score != null)
            Dealer_Score.text = dealerScore.ToString();

        isGameActive = true;
        ClearTable();

        DrawCardForPlayer();
        DrawCardForDealer();
        DrawCardForPlayer();

        if (Cards != null && Cards.cards.Count > 52)
            ShowDealerCard(52);

        if (playerScore == 21)
            Stand();
    }

    public void Zmiana(float wartosc)
    {
        if (isGameActive)
            return;

        aktualnaStawka = Mathf.RoundToInt(wartosc);

        if (BetScore != null)
            BetScore.text = aktualnaStawka.ToString();
    }

    public void Hit()
    {
        if (!isGameActive)
            return;

        isHit = true;
        DrawCardForPlayer(allowLuck: true);

        if (playerScore > 21)
        {
            UstawWynikUI($"Fura! Przegrales ${aktualnaStawka}");
            EndGame();
        }
        else if (playerScore == 21)
        {
            Stand();
        }
    }

    public void Double()
    {
        if (!isGameActive || isHit)
            return;

        if (SaldoGracza < aktualnaStawka)
        {
            UstawWynikUI("Za malo srodkow na Double!");
            return;
        }

        SaldoGracza -= aktualnaStawka;
        aktualnaStawka *= 2;

        if (BetScore != null)
            BetScore.text = aktualnaStawka.ToString();

        DrawCardForPlayer();

        if (playerScore > 21)
        {
            UstawWynikUI($"Fura! Przegrales ${aktualnaStawka}");
            EndGame();
        }
        else
        {
            Stand();
        }
    }

    public void Stand()
    {
        if (!isGameActive)
            return;

        DealerTurn();
    }

    public void DealerTurn()
    {
        if (spawnedDealerCards.Count > 1 &&
            Cards != null &&
            Cards.cards.Count > 52 &&
            spawnedDealerCards[1].sprite == Cards.cards[52])
        {
            Destroy(spawnedDealerCards[1].gameObject);
            spawnedDealerCards.RemoveAt(1);
        }

        while (dealerScore < 17)
        {
            DrawCardForDealer();
        }

        CheckWinner();
    }

    public void DrawCardForPlayer()
    {
        DrawCardForPlayer(allowLuck: false);
    }

    private void DrawCardForPlayer(bool allowLuck)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(2);

        int cardIndex = DrawUniqueCardIndex();
        if (cardIndex == -1)
            return;

        GameRun run = GameRun.Instance;

        if (allowLuck &&
            !luckRerollUsed &&
            run != null &&
            WillPlayerBust(cardIndex) &&
            run.TryBlackjackReroll())
        {
            int replacementIndex = DrawUniqueCardIndex();

            if (replacementIndex != -1)
            {
                // Odrzucona karta wraca do talii po dobraniu zastępczej.
                usedCardIndices.Remove(cardIndex);
                cardIndex = replacementIndex;
                luckRerollUsed = true;
            }
        }

        ShowPlayerCard(cardIndex);
        lastDrawnCardName = GetCardName(cardIndex);

        playerScore += GetCardValue(cardIndex, ref playerAces);

        while (playerScore > 21 && playerAces > 0)
        {
            playerScore -= 10;
            playerAces--;
        }

        if (Player_Score != null)
            Player_Score.text = playerScore.ToString();
    }

    private bool WillPlayerBust(int cardIndex)
    {
        int predictedAces = playerAces;
        int predictedScore = playerScore + GetCardValue(cardIndex, ref predictedAces);

        while (predictedScore > 21 && predictedAces > 0)
        {
            predictedScore -= 10;
            predictedAces--;
        }

        return predictedScore > 21;
    }

    public void DrawCardForDealer()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(2);

        int cardIndex = DrawUniqueCardIndex();
        if (cardIndex == -1)
            return;

        ShowDealerCard(cardIndex);
        lastDrawnCardName = GetCardName(cardIndex);

        dealerScore += GetCardValue(cardIndex, ref dealerAces);

        while (dealerScore > 21 && dealerAces > 0)
        {
            dealerScore -= 10;
            dealerAces--;
        }

        if (Dealer_Score != null)
            Dealer_Score.text = dealerScore.ToString();
    }

    public int DrawUniqueCardIndex()
    {
        if (Cards == null || Cards.cards.Count == 0)
            return -1;

        int playableDeckSize = Mathf.Min(52, Cards.cards.Count);

        if (usedCardIndices.Count >= playableDeckSize)
            return -1;

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, playableDeckSize);
        }
        while (usedCardIndices.Contains(randomIndex));

        usedCardIndices.Add(randomIndex);
        return randomIndex;
    }

    public int GetCardValue(int index, ref int aceCounter)
    {
        int rank = index % 13;

        if (rank == 0)
        {
            aceCounter++;
            return 11;
        }

        if (rank <= 9)
            return rank + 1;

        return 10;
    }

    public string GetCardName(int index)
    {
        string[] suits = { "Clubs", "Spades", "Hearts", "Diamonds" };
        string[] ranks =
        {
            "Ace", "2", "3", "4", "5", "6", "7",
            "8", "9", "10", "Jack", "Queen", "King"
        };

        int suitIndex = index / 13;
        int rankIndex = index % 13;

        if (suitIndex < 0 || suitIndex >= suits.Length)
            return "Unknown";

        return ranks[rankIndex] + " of " + suits[suitIndex];
    }

    public void ShowPlayerCard(int cardIndex)
    {
        Image targetCard;

        if (spawnedPlayerCards.Count == 0)
        {
            targetCard = PlayerCard1;
        }
        else
        {
            targetCard = Instantiate(PlayerCard1, PlayerCard1.transform.parent);
            RectTransform baseRect = PlayerCard1.GetComponent<RectTransform>();
            RectTransform newRect = targetCard.GetComponent<RectTransform>();

            newRect.anchoredPosition = new Vector2(
                baseRect.anchoredPosition.x + spawnedPlayerCards.Count * cardOffsetX,
                baseRect.anchoredPosition.y
            );
        }

        targetCard.sprite = Cards.cards[cardIndex];
        targetCard.gameObject.SetActive(true);
        spawnedPlayerCards.Add(targetCard);
    }

    public void ShowDealerCard(int cardIndex)
    {
        Image targetCard;

        if (spawnedDealerCards.Count == 0)
        {
            targetCard = DealerCard1;
        }
        else
        {
            targetCard = Instantiate(DealerCard1, DealerCard1.transform.parent);
            RectTransform baseRect = DealerCard1.GetComponent<RectTransform>();
            RectTransform newRect = targetCard.GetComponent<RectTransform>();

            newRect.anchoredPosition = new Vector2(
                baseRect.anchoredPosition.x + spawnedDealerCards.Count * cardOffsetX,
                baseRect.anchoredPosition.y
            );
        }

        targetCard.sprite = Cards.cards[cardIndex];
        targetCard.gameObject.SetActive(true);
        spawnedDealerCards.Add(targetCard);
    }

    public void ClearTable()
    {
        for (int i = 1; i < spawnedPlayerCards.Count; i++)
        {
            if (spawnedPlayerCards[i] != null)
                Destroy(spawnedPlayerCards[i].gameObject);
        }

        spawnedPlayerCards.Clear();
        PlayerCard1.gameObject.SetActive(false);

        for (int i = 1; i < spawnedDealerCards.Count; i++)
        {
            if (spawnedDealerCards[i] != null)
                Destroy(spawnedDealerCards[i].gameObject);
        }

        spawnedDealerCards.Clear();
        DealerCard1.gameObject.SetActive(false);
    }

    public void CheckWinner()
    {
        if (dealerScore > 21)
        {
            int wygrana = aktualnaStawka * 2;
            SaldoGracza += wygrana;
            UstawWynikUI($"Krupier ma fure! Wygrywasz ${wygrana}");
        }
        else if (playerScore > dealerScore)
        {
            int wygrana = aktualnaStawka * 2;
            SaldoGracza += wygrana;
            UstawWynikUI($"Wygrales ${wygrana}!");
        }
        else if (playerScore < dealerScore)
        {
            UstawWynikUI($"Przegrales ${aktualnaStawka}!");
        }
        else
        {
            SaldoGracza += aktualnaStawka;
            UstawWynikUI("Remis! Zwrot stawki.");
        }

        EndGame();
    }

    public void EndGame()
    {
        isGameActive = false;

        if (Bet_Size != null)
        {
            aktualnaStawka = Mathf.RoundToInt(Bet_Size.value);

            if (BetScore != null)
                BetScore.text = aktualnaStawka.ToString();
        }
    }
}