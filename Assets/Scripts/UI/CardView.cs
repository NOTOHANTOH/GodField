using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System.Linq;


public class CardView : MonoBehaviour
{
    public TextMeshProUGUI valueText;
    public Button button;


    public Image iconImage;

    public GameObject selected;

    private Card _card;
    public Subject<Card> OnClick = new Subject<Card>();

    public void Setup(Card card, IReadOnlyReactiveCollection<Card> selectedAttack, IReadOnlyReactiveCollection<Card> selectedDefense)
    {
        _card = card;

        if (card.IsNormalAttack)
        {
            valueText.text = "攻"+card.Attack.ToString();
        }
        if (card.IsAttackBoost)
        {
            valueText.text = "+" + card.BonusDamage.ToString();
        }
        if (card.IsDefense)
        {
            valueText.text = "守"+card.Defense.ToString();
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
        button.onClick.AddListener(() => OnClick.OnNext(_card));

        Observable.Merge(
            selectedAttack.ObserveCountChanged(true).AsUnitObservable(),
            selectedDefense.ObserveCountChanged(true).AsUnitObservable()
        )
        .Subscribe(_ =>
        {
            bool isSelected = selectedAttack.Contains(_card) || selectedDefense.Contains(_card);
            selected.SetActive(isSelected);
        })
        .AddTo(this);
    }
}