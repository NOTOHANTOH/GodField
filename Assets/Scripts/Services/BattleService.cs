using System.Collections.Generic;
using UnityEngine;

public class BattleService
{
    // 防御カードを複数枚受け取れるように List<Card> に変更
    public void Resolve(Player defender, Card attackCard, List<Card> defenseCards)
    {
        int atk = attackCard.Attack;
        int def = 0;

        // 防御カードの合計値を計算
        if (defenseCards != null)
        {
            foreach (var card in defenseCards)
            {
                def += card.Defense;
            }
        }

        int damage = Mathf.Max(0, atk - def);
        defender.Hp.Value -= damage;

        Debug.Log($"バトル結果: 攻撃 {atk} - 防御 {def} = {damage}ダメージ！");
    }
}