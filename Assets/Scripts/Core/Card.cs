using UnityEngine;

public class Card
{
    public CardData Data;

    public Card(CardData data)
    {
        Data = data;
    }

    public int Attack => Data.GetAttack();
    public int Defense => Data.GetDefense();
    public string Name => Data.cardName;

    public Sprite Image => Data.cardImage;

    // 型判定を追加（これで攻撃カードか防御カードか見分ける）
    public bool IsAttack => Data is AttackCardData;
    public bool IsDefense => Data is DefenseCardData;
}