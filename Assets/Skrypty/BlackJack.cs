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
    void Awake()
    {
        Cards = GetComponent<Cards>();
    }


}
