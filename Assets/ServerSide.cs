using UnityEngine;
using System.Collections;

[RequireComponent( typeof( NetworkView ) )] // говорит Unity о том что нам нужен компонент NetworkView, данному компоненту NetworkStateSynchronization можно выставить Off

public class ServerSide : MonoBehaviour {
	private int playerCount = 0; // хранит количество подключенных игроков
	public int PlayersCount { get { return playerCount; } } // гетер для внешних компонентов о количестве игроков на сервере

    void OnServerInitialized() {
        SendMessage( "SpawnPlayer", "Player Server" ); // создаем локального игрока сервера
    }

    void OnPlayerConnected( NetworkPlayer player ) {
		++playerCount; // при подключении игрока увеличиваем количество подключенных игроков
        networkView.RPC( "SpawnPlayer", player, "Player " + playerCount.ToString() ); // вызываем у игрока процедуру создания экземпляяра прифаба
    }

    void OnPlayerDisconnected( NetworkPlayer player ) {
        --playerCount; // уменьшаем количество игроков
        Network.RemoveRPCs( player ); // очищаем список процедур игрока
        Network.DestroyPlayerObjects( player ); // уничтожаем все обьекты игрока
    }
}