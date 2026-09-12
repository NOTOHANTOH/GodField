using UnityEngine;

[CreateAssetMenu(menuName = "Card/Defense")]
public class DefenseCardData : CardData
{
    public override int GetAttack() => 0;
    public override int GetDefense() => value;
}