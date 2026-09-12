using UnityEngine;

[CreateAssetMenu(menuName = "Card/Attack")]
public class AttackCardData : CardData
{
    [Header("ダメージ増加カード用")]
    public int bonusDamage = 0;
    public bool IsBoostCard => bonusDamage > 0;

    public override int GetAttack() => value;
    public override int GetDefense() => 0;

    
}