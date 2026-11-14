using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BlackJack : MonoBehaviour
{
    public Cards Cards;
    public Image PlayerCard1;
    public Image DealerCard1;
    public int bet = 100;
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

    void Awake()
    {
        Cards = GetComponent<Cards>();
    }
    public void Bet()
    {
        if (isGameActive) return;

        usedCardIndices.Clear();
        playerScore = 0;
        dealerScore = 0;
        playerAces = 0;
        dealerAces = 0;
        isGameActive = true;
        ClearTable();

        DrawCardForPlayer();
        DrawCardForDealer();
        DrawCardForPlayer();
        DrawCardForDealer();

        if (playerScore == 21)
        {
            Stand();
        }
        else if(dealerScore == 21){
            EndGame();

        }
    }

    public void Hit()
    {
        if (!isGameActive) return;

        DrawCardForPlayer();

        if (playerScore > 21)
        {
            EndGame();
        }
        else if (playerScore == 21)
        {
            Stand();
        }
    }

    public void Double()
    {
        if (!isGameActive) return;

        bet *= 2;
        DrawCardForPlayer();

        if (playerScore > 21)
        {
            EndGame();
        }
        else
        {
            Stand();
        }
    }

    public void Stand()
    {
        if (!isGameActive) return;

        DealerTurn();
    }

    public void DealerTurn()
    {
        while (dealerScore < 17)
        {
            DrawCardForDealer();
        }

        CheckWinner();
    }

    public void DrawCardForPlayer()
    {
        int cardIndex = DrawUniqueCardIndex();
        if (cardIndex == -1) return;

        ShowPlayerCard(cardIndex);
        lastDrawnCardName = GetCardName(cardIndex);
        int value = GetCardValue(cardIndex, ref playerAces);
        playerScore += value;

        while (playerScore > 21 && playerAces > 0)
        {
            playerScore -= 10;
            playerAces--;
        }
    }

    public void DrawCardForDealer()
    {
        int cardIndex = DrawUniqueCardIndex();
        if (cardIndex == -1) return;

        ShowDealerCard(cardIndex);
        lastDrawnCardName = GetCardName(cardIndex);
        int value = GetCardValue(cardIndex, ref dealerAces);
        dealerScore += value;

        while (dealerScore > 21 && dealerAces > 0)
        {
            dealerScore -= 10;
            dealerAces--;
        }
    }

    public int DrawUniqueCardIndex()
    {
        if (Cards == null || Cards.cards.Count == 0) return -1;
        if (usedCardIndices.Count >= Cards.cards.Count) return -1;

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, Cards.cards.Count);
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

        if (rank <= 9) return rank + 1;

        return 10;
    }

    public string GetCardName(int index)
    {
        string[] suits = { "Clubs", "Spades", "Hearts", "Diamonds" };
        string[] ranks = { "Ace", "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King" };

        int suitIndex = index / 13;
        int rankIndex = index % 13;

        if (suitIndex < 0 || suitIndex >= suits.Length) return "Unknown";

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
            newRect.anchoredPosition = new Vector2(baseRect.anchoredPosition.x + (spawnedPlayerCards.Count * cardOffsetX), baseRect.anchoredPosition.y);
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
            newRect.anchoredPosition = new Vector2(baseRect.anchoredPosition.x + (spawnedDealerCards.Count * cardOffsetX), baseRect.anchoredPosition.y);
        }

        targetCard.sprite = Cards.cards[cardIndex];
        targetCard.gameObject.SetActive(true);
        spawnedDealerCards.Add(targetCard);
    }

    public void ClearTable()
    {
        for (int i = 1; i < spawnedPlayerCards.Count; i++)
        {
            if (spawnedPlayerCards[i] != null) Destroy(spawnedPlayerCards[i].gameObject);
        }
        spawnedPlayerCards.Clear();
        PlayerCard1.gameObject.SetActive(false);

        for (int i = 1; i < spawnedDealerCards.Count; i++)
        {
            if (spawnedDealerCards[i] != null) Destroy(spawnedDealerCards[i].gameObject);
        }
        spawnedDealerCards.Clear();
        DealerCard1.gameObject.SetActive(false);
    }

    public void CheckWinner()
    {
        if (dealerScore > 21)
        {
        
        }
        else if (playerScore > dealerScore)
        {
        
        }
        else if (playerScore < dealerScore)
        {
        
        }
        else
        {
        
        }

        EndGame();
    }

    public void EndGame()
    {
        isGameActive = false;
    }
}