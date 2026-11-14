using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BlackJack : MonoBehaviour
{
    public Cards Cards;
    public Image PlayerCard1;
    public Image PlayerCard2;
    public Image DealerCard1;
    public Image DealerCard2;
    public int bet = 100;
    public bool isGameActive = false;
    public int playerScore = 0;
    public int dealerScore = 0;
    public int playerAces = 0;
    public int dealerAces = 0;
    public List<int> usedCardIndices = new List<int>();
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

        DrawCardForPlayer();
        DrawCardForDealer();
        DrawCardForPlayer();
        DrawCardForDealer();

        if (playerScore == 21)
        {
            Stand();
        }
        else if(dealerScore == 21){
            CheckWinner();

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

        if (rank <= 8) return rank + 2;
        if (rank <= 11) return 10;

        aceCounter++;
        return 11;
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
