using UnityEngine;

[CreateAssetMenu(menuName = "Card/Attack")]
public class AttackCardData : CardData
{
    public override int GetAttack() => value;
    public override int GetDefense() => 0;
}