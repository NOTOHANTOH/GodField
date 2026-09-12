using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class FieldView : MonoBehaviour
{
    public SelectedCardView selectedCardPrefab;
    public Transform parent;


    public void Bind(ReactiveCollection<Card> hand,  System.Action<Card> onClick)
    {

    }

    public void Show(List<Card> hand, System.Action<Card> onClick)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        foreach (var card in hand)
        {
            var view = Instantiate(selectedCardPrefab, parent);
        }
    }
}
