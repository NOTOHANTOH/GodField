using UniRx;
using Cysharp.Threading.Tasks;
using VContainer;
using UnityEngine;
using System.Collections.Generic;

public class GameController
{
    private readonly DeckService _deckService;
    private readonly BattleService _battleService;

    public ReactiveProperty<int> TurnCount { get; } = new ReactiveProperty<int>(1);

    public Player Player1 { get; private set; }
    public Player Player2 { get; private set; }

    // ReactiveCollectionを使うと、増減した時にUI側で検知できる
    public ReactiveCollection<Card> Hand1 { get; private set; } = new ReactiveCollection<Card>();
    public ReactiveCollection<Card> Hand2 { get; private set; } = new ReactiveCollection<Card>();

    public ReactiveProperty<Card> FocusedCard { get; } = new ReactiveProperty<Card>();

    // 現在の状況をUIに伝えるためのプロパティ
    public ReactiveProperty<string> PhaseMessage { get; private set; } = new ReactiveProperty<string>("準備中");

    // カードが選ばれるのを「待つ」ための仕組み
    private UniTaskCompletionSource<Card> _cardSelectTcs;

    [Inject]
    public GameController(DeckService deckService, BattleService battleService)
    {
        _deckService = deckService;
        _battleService = battleService;
    }

    public void Initialize()
    {
        Player1 = new Player(20, 50, 100);
        Player2 = new Player(20, 50, 100);

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

        // ① ドロー
        PhaseMessage.Value = $"{activeName}のターン: ドロー";
        turnPlayerHand.Add(_deckService.DrawCard());
        await UniTask.Delay(1000); // UIを見せるためのタメ

        // ② 攻撃カードの選択待ち
        PhaseMessage.Value = $"{activeName}: 攻撃カードを出してください";
        Card attackCard = await WaitForCardSelect(turnPlayerHand, true);
        turnPlayerHand.Remove(attackCard);

        // ③ 防御カードの選択待ち（出さない場合は null になる）
        PhaseMessage.Value = $"{targetName}: 防御カードを出してください（スキップ可）";
        List<Card> playedDefenseCards = new List<Card>();

        while (true)
        {
            Card defenseCard = await WaitForCardSelect(defenderHand, false);
            if (defenseCard == null) break; // スキップ（決定ボタン等）で防御フェーズ終了

            playedDefenseCards.Add(defenseCard);
            defenderHand.Remove(defenseCard);

            // 更に防御カードを重ねられるようにメッセージを更新
            PhaseMessage.Value = $"{targetName}: さらに防御カードを出せます（完了可）";
        }

        // ④ バトルの解決
        PhaseMessage.Value = "バトル解決！";
        _battleService.Resolve(defender, attackCard, playedDefenseCards);
        await UniTask.Delay(1500);
    }

    // UIのカードがクリックされたら呼ばれる
    public void SelectCard(Card card)
    {
        if (FocusedCard.Value == card)
        {
            FocusedCard.Value = null;
        }
        else
        {
            FocusedCard.Value = card;
        }
    }

    public void DecideCard()
    {
        _cardSelectTcs?.TrySetResult(FocusedCard.Value);
    }

    // UIのスキップボタンがクリックされたら呼ばれる
    public void SkipDefense()
    {
        _cardSelectTcs?.TrySetResult(null);
    }

    // 指定した条件のカードが出されるまで待機する関数
    private async UniTask<Card> WaitForCardSelect(ReactiveCollection<Card> hand, bool wantAttack)
    {
        while (true)
        {
            FocusedCard.Value = null; // 選択開始時にフォーカスをリセット

            _cardSelectTcs = new UniTaskCompletionSource<Card>();
            var card = await _cardSelectTcs.Task; // 決定ボタンが押されるまで待機


            if (card == null)
            {
                if (!wantAttack)
                {
                    return null; // 防御フェーズならスキップ確定
                }
                else
                {
                    PhaseMessage.Value = "攻撃カードを選択してください！";
                    continue; // 攻撃フェーズならやり直し
                }
            }
            if (hand.Contains(card))
            {
                if (wantAttack && card.IsAttack) return card;
                if (!wantAttack && card.IsDefense) return card;
            }

            // 間違ったカードならエラーメッセージを出して再度待つ
            PhaseMessage.Value = wantAttack ? "攻撃カードを選んでください！" : "防御カードを選んでください！";
            FocusedCard.Value = null; // フォーカスを外す
        }
    }
}