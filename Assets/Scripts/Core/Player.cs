using UniRx;

public class Player
{
    public ReactiveProperty<int> Hp { get; }
    public ReactiveProperty<int> Mp { get; }
    public ReactiveProperty<int> Money { get; }

    public Player(int hp, int mp, int money)
    {
        Hp = new ReactiveProperty<int>(hp);
        Mp = new ReactiveProperty<int>(mp);
        Money = new ReactiveProperty<int>(money);
    }
}