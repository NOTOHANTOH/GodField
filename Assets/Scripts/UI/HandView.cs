using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class HandView : MonoBehaviour
{
    public CardView cardPrefab;
    public Transform parent;

    private ReactiveProperty<Card> _focusedCard;

    public void Bind(ReactiveCollection<Card> hand, ReactiveCollection<Card> selectedAttack, ReactiveCollection<Card> selectedDefense, System.Action<Card> onClick)
    {
        hand.ObserveCountChanged(true)
            .Subscribe(_ => Show(hand.ToList(), selectedAttack, selectedDefense, onClick))
            .AddTo(this);
    }

    public void Show(List<Card> hand, ReactiveCollection<Card> selectedAttack, ReactiveCollection<Card> selectedDefense, System.Action<Card> onClick)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        foreach (var card in hand)
        {
            var view = Instantiate(cardPrefab, parent);
            view.Setup(card, selectedAttack, selectedDefense);
            view.OnClick.Subscribe(onClick).AddTo(view);
        }
    }
}