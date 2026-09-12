using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectedCardView : MonoBehaviour
{
    [SerializeField] private Image cardIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;

    public void Setup(Card card)
    {
        nameText.text = card.Name;
        cardIcon.sprite = card.Image;
        costText.text = $"Åê{card.Data.cost}";

        if (card.IsAttack)
            descriptionText.text = $"çU {card.Attack}";
        else if (card.IsDefense)
            descriptionText.text = $"ñh {card.Defense}";
    }
}