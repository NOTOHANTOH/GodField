using UnityEngine;

public abstract class CardData : ScriptableObject
{
    public string cardName;
    public int value;
    public int cost;

    public Sprite cardImage;

    public abstract int GetAttack();
    public abstract int GetDefense();
}