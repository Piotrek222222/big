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
    public Text Player_Score;
    public Text Dealer_Score;
    public bool isHit;
    public string luckPrefsKey = "PlayerLuck";
    public float defaultLuck = 0f;
    public float luck;

    void Awake()
    {
        Cards = GetComponent<Cards>();
        PlayerCard1.gameObject.SetActive(false);
        DealerCard1.gameObject.SetActive(false);

        if (luck <= 0f)
            luck = PlayerPrefs.GetFloat(luckPrefsKey, defaultLuck);
    }

    public void Bet()
    {
        if (isGameActive) return;

        if (luck <= 0f && PlayerPrefs.HasKey(luckPrefsKey))
            luck = PlayerPrefs.GetFloat(luckPrefsKey, defaultLuck);

        PlayerCard1.gameObject.SetActive(true);
        DealerCard1.gameObject.SetActive(true);
        isHit = false;
        usedCardIndices.Clear();
        playerScore = 0;
        dealerScore = 0;
        if (Player_Score != null) Player_Score.text = playerScore.ToString();
        if (Dealer_Score != null) Dealer_Score.text = dealerScore.ToString();
        playerAces = 0;
        dealerAces = 0;
        isGameActive = true;
        ClearTable();

        DrawCardForPlayer();
        DrawCardForDealer();
        DrawCardForPlayer();
        if (Cards != null && Cards.cards.Count > 52) ShowDealerCard(52);

        if (playerScore == 21)
        {
            Stand();
        }
    }

    public void Hit()
    {
        if (!isGameActive) return;
        isHit = true;

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
        if (isHit) return;
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
        if (spawnedDealerCards.Count > 1 && Cards != null && Cards.cards.Count > 52 && spawnedDealerCards[1].sprite == Cards.cards[52])
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
        int cardIndex = DrawUniqueCardIndex();
        if (cardIndex == -1) return;

        if (WillPlayerBust(cardIndex) && Random.Range(0f, 100f) < luck)
        {
            int safeCardIndex = DrawSafeCardIndexForPlayer();
            if (safeCardIndex != -1)
            {
                usedCardIndices.Remove(cardIndex);
                cardIndex = safeCardIndex;
            }
        }

        ShowPlayerCard(cardIndex);
        lastDrawnCardName = GetCardName(cardIndex);
        int value = GetCardValue(cardIndex, ref playerAces);
        playerScore += value;

        while (playerScore > 21 && playerAces > 0)
        {
            playerScore -= 10;
            playerAces--;
        }

        if (Player_Score != null) Player_Score.text = playerScore.ToString();
    }

    private bool WillPlayerBust(int cardIndex)
    {
        int tempAces = playerAces;
        int value = GetCardValue(cardIndex, ref tempAces);
        int predictedScore = playerScore + value;

        while (predictedScore > 21 && tempAces > 0)
        {
            predictedScore -= 10;
            tempAces--;
        }

        return predictedScore > 21;
    }

    private int DrawSafeCardIndexForPlayer()
    {
        if (Cards == null || Cards.cards.Count == 0) return -1;
        int playableDeckSize = Mathf.Min(52, Cards.cards.Count);

        List<int> safeCandidates = new List<int>();
        for (int i = 0; i < playableDeckSize; i++)
        {
            if (!usedCardIndices.Contains(i) && !WillPlayerBust(i))
            {
                safeCandidates.Add(i);
            }
        }

        if (safeCandidates.Count == 0) return -1;

        int chosenIndex = safeCandidates[Random.Range(0, safeCandidates.Count)];
        usedCardIndices.Add(chosenIndex);
        return chosenIndex;
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

        if (Dealer_Score != null) Dealer_Score.text = dealerScore.ToString();
    }

    public int DrawUniqueCardIndex()
    {
        if (Cards == null || Cards.cards.Count == 0) return -1;
        int playableDeckSize = Mathf.Min(52, Cards.cards.Count);
        if (usedCardIndices.Count >= playableDeckSize) return -1;

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