using System.Collections.Generic;
using UnityEngine;

public class DeckService
{
    private readonly List<CardData> _masterCards;

    public DeckService(List<CardData> masterCards)
    {
        _masterCards = masterCards;
    }

    // 全カードからランダムに1枚引く
    public Card DrawCard()
    {
        int index = Random.Range(0, _masterCards.Count);
        Debug.Log(_masterCards[index]+"をひいた！");
        return new Card(_masterCards[index]);
    }
}