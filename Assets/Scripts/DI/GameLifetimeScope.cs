using UnityEngine;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    // インスペクターから全てのカードデータ(ScriptableObject)をリストで入れる
    [SerializeField] private List<CardData> cardDatas;

    // シーン上に配置したGameSceneManagerを紐付ける
    [SerializeField] private GameSceneManager gameSceneManager;

    protected override void Configure(IContainerBuilder builder)
    {
        // 1. マスターデータの登録
        // DeckServiceがカード生成時に使うリストを渡せるようにする
        builder.RegisterInstance(cardDatas);

        // 2. プレーンなC#クラス（ロジック層）の登録
        // Singletonにすることで、ゲーム中に1つだけ実体が作られ、使い回される
        builder.Register<DeckService>(Lifetime.Singleton);
        builder.Register<BattleService>(Lifetime.Singleton);
        builder.Register<GameController>(Lifetime.Singleton);

        // 3. シーン上のMonoBehaviour（表示・進行層）を登録
        // [Inject] メソッドを呼び出してもらうために必要
        builder.RegisterComponent(gameSceneManager);
    }
}