using UniRx;

public class Player
{
    public ReactiveProperty<string> Name { get; set; }

    public ReactiveProperty<int> Hp { get; }
    public ReactiveProperty<int> Mp { get; }
    public ReactiveProperty<int> Money { get; }

    public ReactiveProperty<Curse> Curse { get; }

    public Player(string name, int hp, int mp, int money)
    {
        Name = new ReactiveProperty<string>(name);
        Hp = new ReactiveProperty<int>(hp);
        Mp = new ReactiveProperty<int>(mp);
        Money = new ReactiveProperty<int>(money);
    }
}

public enum Curse
{
    None,
    Cold,
    Fever,
    Hell,
    Heaven,
    Fog,
    Flash,
    Dream,
    DarkCloud
}