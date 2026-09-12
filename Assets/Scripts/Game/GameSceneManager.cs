using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Linq;

public class GameSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private PlayerStatusView p1StatusView;
    [SerializeField] private PlayerStatusView p2StatusView;
    [SerializeField] private HandView p1HandView;
    [SerializeField] private HandView p2HandView;
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private Button attackerButton;
    [SerializeField] private Button defenderButton;
    [SerializeField] private SelectedCardListView attackSelectedListView;
    [SerializeField] private SelectedCardListView defenseSelectedListView;


    [Inject]
    public void Construct(GameController gameController)
    {
        // ① データの準備
        gameController.Initialize();

        // ② 各UIにデータを渡す（Bind）
        p1StatusView.Bind(gameController.Player1);
        p2StatusView.Bind(gameController.Player2);

        p1HandView.Bind(gameController.Hand1, gameController.SelectedAttackCards, gameController.SelectedDefenseCards, gameController.SelectCard);
        p2HandView.Bind(gameController.Hand2, gameController.SelectedAttackCards, gameController.SelectedDefenseCards, gameController.SelectCard);

        attackSelectedListView.Bind(gameController.SelectedAttackCards); // 前回作った SelectedCardListView をそのまま流用
        defenseSelectedListView.Bind(gameController.SelectedDefenseCards);

        gameController.ActiveHandIsP1
            .Subscribe(isP1 => SwitchHandDisplay(isP1 ? 1 : 2))
            .AddTo(this);

        // 決定ボタンは1つに統合可能。両方残すなら両方に同じメソッドを紐付け
        attackerButton.onClick.AddListener(() => gameController.DecideCard());
        defenderButton.onClick.AddListener(() => gameController.DecideCard()); // SkipDefenseは廃止

        gameController.PhaseMessage.Subscribe(msg => phaseText.text = msg).AddTo(this);


        gameController.TurnCount.Subscribe(count => turnText.text = count.ToString()).AddTo(this);

        gameController.IsP1Turn
            .Subscribe(isP1 => SwitchHandDisplay(isP1 ? 1 : 2))
            .AddTo(this);

        // ③ ゲームループ開始！
        gameController.StartGameLoop().Forget();


    }

    private void Update()
    {
        // テンキーではない上の数字キー「1」が押されたら
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchHandDisplay(1);
        }
        // 数字キー「2」が押されたら
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchHandDisplay(2);
        }
    }

    private void SwitchHandDisplay(int playerNumber)
    {
        if (playerNumber == 1)
        {
            p1HandView.gameObject.SetActive(true);
            p2HandView.gameObject.SetActive(false);
        }
        else if (playerNumber == 2)
        {
            p1HandView.gameObject.SetActive(false);
            p2HandView.gameObject.SetActive(true);
        }
    }
}