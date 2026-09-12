using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class SelectedCardListView : MonoBehaviour
{
    public SelectedCardView cardPrefab;
    public Transform parent;

    public void Bind(ReactiveCollection<Card> cards)
    {
        cards.ObserveCountChanged(true)
            .Subscribe(_ => Show(cards.ToList()))
            .AddTo(this);
    }

    private void Show(List<Card> cards)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        foreach (var card in cards)
        {
            var view = Instantiate(cardPrefab, parent);
            view.Setup(card);
        }
    }
}