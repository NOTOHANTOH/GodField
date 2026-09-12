using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class SelectedCardView : MonoBehaviour
{
    [SerializeField] private GameObject contentRoot; // 全体を消したり出したりする用
    [SerializeField] private Image cardIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText; // 「攻撃」「防御」の表示用
    [SerializeField] private TextMeshProUGUI costText;


    public void Bind(ReactiveProperty<Card> focusedCard)
    {
        focusedCard.Subscribe(card =>
        {
            if (card == null)
            {
                contentRoot.SetActive(false); // 何も選ばれていなければ非表示
                return;
            }

            contentRoot.SetActive(true);
            UpdateUI(card);
        }).AddTo(this);
    }

    private void UpdateUI(Card card)
    {
        nameText.text = card.Name;
        cardIcon.sprite = card.Image;
        costText.text = $"Cost: {card.Data.cost}";

        // 攻撃か防御かで表示を切り替える
        if (card.IsAttack)
        {
            descriptionText.text = $"ATK: {card.Attack}";
        }
        else if (card.IsDefense)
        {
            descriptionText.text = $"DEF: {card.Defense}";
        }
    }
}