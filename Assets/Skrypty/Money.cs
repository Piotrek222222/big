using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;

    private void LateUpdate()
    {
        if (moneyText != null)
        {
            int money = GameData.PobierzLubUtworz().SaldoGracza;
            moneyText.text = $"${money}";
        }
    }
}
