using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;


public class CardView : MonoBehaviour
{
    public TextMeshProUGUI valueText;
    public Button button;


    public Image iconImage;

    public GameObject selected;

    private Card _card;
    public Subject<Card> OnClick = new Subject<Card>();

    public void Setup(Card card, ReactiveProperty<Card> focusedCard)
    {
        _card = card;

        if (card.IsAttack)
        {
            valueText.text = "攻"+card.Attack.ToString();
        }
        if (card.IsDefense)
        {
            valueText.text = "防"+card.Defense.ToString();
        }


        iconImage.sprite = card.Image;

        // （任意）画像が設定されていない時は透明にする等の処理
        if (card.Image == null)
        {
            iconImage.color = new Color(0, 0, 0, 0); // 透明
        }
        else
        {
            iconImage.color = Color.white; // 通常表示
        }

        // ※以前のリスナー登録が重複しないよう、一度クリアするのが安全です
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            OnClick.OnNext(_card);
        });

        focusedCard.Subscribe(focused =>
        {
            bool isSelected = (focused == _card);
            selected.SetActive(isSelected);
        }).AddTo(this);
    }
}