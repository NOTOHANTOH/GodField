using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class HandView : MonoBehaviour
{
    public CardView cardPrefab;
    public Transform parent;

    private ReactiveProperty<Card> _focusedCard;

    public void Bind(ReactiveCollection<Card> hand, ReactiveProperty<Card> focusedCard, System.Action<Card> onClick)
    {
        _focusedCard = focusedCard;

        // ŽèŽD‚ª‘Œ¸‚µ‚½‚çAŽ©“®‚ÅShow‚ðŒÄ‚Ño‚µ‚ÄÄ•`‰æ‚·‚é
        hand.ObserveCountChanged(true)
            .Subscribe(_ => Show(hand.ToList(), onClick))
            .AddTo(this);
    }

    public void Show(List<Card> hand, System.Action<Card> onClick)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        foreach (var card in hand)
        {
            var view = Instantiate(cardPrefab, parent);
            view.Setup(card,_focusedCard);
            view.OnClick.Subscribe(onClick).AddTo(view);
        }
    }
}