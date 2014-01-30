using UnityEngine;
using System.Collections;

[RequireComponent( typeof( NetworkView ) )] // говорит Unity о том что нам нужен компонент NetworkView, данному компоненту NetworkStateSynchronization можно выставить Off

public class ClientSide : MonoBehaviour {
    public GameObject playerPrefab; // прифаб игрока, который будет создаваться в процессе игры
    public Vector2 spawnArea = new Vector2( 8.0f, 8.0f ); // зона спауна

    private Vector3 RandomPosition { // случайная позиция в зоне спауна
        get {
            return transform.position +
                    transform.right * ( Random.Range( 0.0f, spawnArea.x ) - spawnArea.x * 0.5f ) +
                    transform.forward * ( Random.Range( 0.0f, spawnArea.y ) - spawnArea.y * 0.5f );
        }
    }

    [RPC] // говорит Unity о том что данный метод можно вызвать из сети
    private void SpawnPlayer( string playerName ) {
        Vector3 position = RandomPosition; // делаем рандомную позицию создания персонвжа
        GameObject newPlayer = Network.Instantiate( playerPrefab, position, Quaternion.LookRotation( transform.position - position, Vector3.up ), 0 ) as GameObject; // создаем нового персонажа в сети
        newPlayer.BroadcastMessage( "SetPlayerName", playerName ); // задаем ему имя ( оно автоматически будет синхронизировано по сети )
    }

    void OnDisconnectedFromServer( NetworkDisconnection info ) {
        Network.DestroyPlayerObjects( Network.player ); // удаляемся из игры
    }
}