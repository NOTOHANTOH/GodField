using UniRx;
using Cysharp.Threading.Tasks;
using VContainer;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameController
{
    private readonly DeckService _deckService;
    private readonly BattleService _battleService;

    public ReactiveProperty<int> TurnCount { get; } = new ReactiveProperty<int>(1);

    public Player Player1 { get; private set; }
    public Player Player2 { get; private set; }

    public ReactiveProperty<bool> IsP1Turn { get; } = new ReactiveProperty<bool>(true);

    // ReactiveCollectionを使うと、増減した時にUI側で検知できる
    public ReactiveCollection<Card> Hand1 { get; private set; } = new ReactiveCollection<Card>();
    public ReactiveCollection<Card> Hand2 { get; private set; } = new ReactiveCollection<Card>();

    public ReactiveProperty<Card> FocusedCard { get; } = new ReactiveProperty<Card>();
    public ReactiveCollection<Card> SelectedAttackCards { get; } = new ReactiveCollection<Card>();
    public ReactiveCollection<Card> SelectedDefenseCards { get; } = new ReactiveCollection<Card>();

    private UniTaskCompletionSource<Card> _cardSelectTcs; // 攻撃用の決定待ち
    private UniTaskCompletionSource _defenseDecideTcs;    // 防御用の決定待ち(戻り値不要、シグナルのみ)

    // どちらのプレイヤーの手札を今操作可能にするか(手番とは別概念)
    public ReactiveProperty<bool> ActiveHandIsP1 { get; } = new ReactiveProperty<bool>(true);

    // 現在の状況をUIに伝えるためのプロパティ
    public ReactiveProperty<string> PhaseMessage { get; private set; } = new ReactiveProperty<string>("準備中");

    private bool _isDefensePhase = false;

    private UniTaskCompletionSource _decideTcs;

    [Inject]
    public GameController(DeckService deckService, BattleService battleService)
    {
        _deckService = deckService;
        _battleService = battleService;
    }

    public void Initialize()
    {
        Player1 = new Player("A",20, 50, 100);
        Player2 = new Player("B", 20, 50, 100);

        TurnCount.Value = 1;

        Debug.Log("initializing...");
    }

    // ゲームのメインループ
    public async UniTask StartGameLoop()
    {

        // 最初の手札を3枚ずつ配る
        for (int i = 0; i < 3; i++) { Hand1.Add(_deckService.DrawCard()); Hand2.Add(_deckService.DrawCard()); }

        // どちらかのHPが0になるまでターンを繰り返す
        while (Player1.Hp.Value > 0 && Player2.Hp.Value > 0)
        {
            await PlayTurn(isP1Turn: true);

            TurnCount.Value++;

            if (Player2.Hp.Value <= 0) break;

            await PlayTurn(isP1Turn: false);

            TurnCount.Value++;
        }

        PhaseMessage.Value = "ゲーム終了！";
    }

    private async UniTask PlayTurn(bool isP1Turn)
    {
        var turnPlayerHand = isP1Turn ? Hand1 : Hand2;
        var defenderHand = isP1Turn ? Hand2 : Hand1;
        var defender = isP1Turn ? Player2 : Player1;
        string activeName = isP1Turn ? "Player1" : "Player2";
        string targetName = isP1Turn ? "Player2" : "Player1";

        //ターン開始時のドローは廃止

        // ②攻撃カードの選択・決定待ち
        _isDefensePhase = false;
        ActiveHandIsP1.Value = isP1Turn; // 攻撃側の手札を表示
        SelectedAttackCards.Clear();
        PhaseMessage.Value = $"{activeName}: 攻撃カードを選んで決定してください";
        List<Card> attackCards = await WaitForAttackSelect(turnPlayerHand);

        if (attackCards == null)
        {
            PhaseMessage.Value = $"{activeName}は攻撃カードがなくターンをスキップしました";
            await UniTask.Delay(1000);
            SelectedAttackCards.Clear();
            return;
        }

        foreach (var c in attackCards)
            turnPlayerHand.Remove(c);

        // ③防御カードの選択・決定待ち(複数選択可)
        _isDefensePhase = true;
        ActiveHandIsP1.Value = !isP1Turn;
        SelectedDefenseCards.Clear();
        PhaseMessage.Value = $"{targetName}: 防御カードを選んで決定してください(0枚でスキップ)";
        List<Card> playedDefenseCards = await WaitForDefenseSelect(defenderHand);

        foreach (var c in playedDefenseCards)
            defenderHand.Remove(c);

        // ④バトルの解決
        PhaseMessage.Value = "バトル解決!";
        _battleService.Resolve(defender, attackCards, playedDefenseCards);
        await UniTask.Delay(1000);

        // ④ターン終了時に使用枚数分ドロー
        for (int i = 0; i < attackCards.Count; i++)
            turnPlayerHand.Add(_deckService.DrawCard());
        for (int i = 0; i < playedDefenseCards.Count; i++)
            defenderHand.Add(_deckService.DrawCard());

        // ⑤ターン終了時にまとめてクリア
        SelectedAttackCards.Clear();
        SelectedDefenseCards.Clear();
    }

    private async UniTask<List<Card>> WaitForAttackSelect(ReactiveCollection<Card> hand)
    {
        while (true)
        {
            _decideTcs = new UniTaskCompletionSource();
            await _decideTcs.Task;

            var normalCard = SelectedAttackCards.FirstOrDefault(c => c.IsAttack);

            if (normalCard == null)
            {
                bool hasCandidate = hand.Any(c => c.IsAttack);
                if (!hasCandidate) return null; // 通常攻撃カードが手札にない→スキップ確定

                PhaseMessage.Value = "攻撃カードを選んでください!";
                continue;
            }

            // 選択されているカード全て(通常+ブースト)が手札に実在するか確認
            if (SelectedAttackCards.All(c => hand.Contains(c)))
                return SelectedAttackCards.ToList();

            PhaseMessage.Value = "攻撃カードを選んでください!";
            SelectedAttackCards.Clear();
        }
    }

    private async UniTask<List<Card>> WaitForDefenseSelect(ReactiveCollection<Card> hand)
    {
        _decideTcs = new UniTaskCompletionSource();
        await _decideTcs.Task;

        return SelectedDefenseCards.Where(c => hand.Contains(c) && c.IsDefense).ToList();
    }

    public void SelectCard(Card card)
    {
        if (_isDefensePhase)
        {
            ToggleInList(SelectedDefenseCards, card);
            return;
        }

        if (card.IsAttackBoost)
        {
            ToggleInList(SelectedAttackCards, card);
            return;
        }

        if (card.IsNormalAttack)
        {
            if (SelectedAttackCards.Contains(card))
            {
                SelectedAttackCards.Remove(card);
                return;
            }

            // 既存の通常攻撃カードだけ差し替え(ブーストカードは残す)
            var existingNormal = SelectedAttackCards.FirstOrDefault(c => c.IsAttack);
            if (existingNormal != null)
                SelectedAttackCards.Remove(existingNormal);

            SelectedAttackCards.Add(card);
        }
    }

    private void ToggleInList(ReactiveCollection<Card> list, Card card)
    {
        if (list.Contains(card))
            list.Remove(card);
        else
            list.Add(card);
    }

    public void DecideCard()
    {
        _decideTcs?.TrySetResult();
    }

    // UIのスキップボタンがクリックされたら呼ばれる
    public void SkipDefense()
    {
        _cardSelectTcs?.TrySetResult(null);
    }
}