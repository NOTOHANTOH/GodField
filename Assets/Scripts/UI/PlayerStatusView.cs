using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;

public class PlayerStatusView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private TextMeshProUGUI moneyText;

    public void Bind(Player player)
    {
        // ‚Ü‚Æ‚ß‚Äw“ÇiSubscribej
        player.Hp.Subscribe(hp => hpText.text = hp.ToString()).AddTo(this);
        player.Mp.Subscribe(mp => mpText.text = mp.ToString()).AddTo(this);
        player.Money.Subscribe(m => moneyText.text = m.ToString()).AddTo(this);
    }
}