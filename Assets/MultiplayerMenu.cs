using UnityEngine;
using System.Collections;

public class MultiplayerMenu : MonoBehaviour {
    const int NETWORK_PORT = 4585; // сетевой порт
    const int MAX_CONNECTIONS = 20; // максимальное количество входящих подключений
    const bool USE_NAT = false; // использовать нат?

    private string remoteServer = "127.0.0.1"; // адрес сервера (также можно localhost)

    void OnGUI() {
        if ( Network.peerType == NetworkPeerType.Disconnected ) { // если не подключен
            if ( GUILayout.Button( "Start Server" ) ) { // кнопка запустить сервер
                Network.InitializeSecurity(); // инициализируем защиту
                Network.InitializeServer( MAX_CONNECTIONS, NETWORK_PORT, USE_NAT ); // запускаем сервер
            }
            GUILayout.Space( 30f ); // отступ
            remoteServer = GUILayout.TextField( remoteServer ); // поле адреса сервера
            if ( GUILayout.Button( "Connect to server" ) ) { // кнопка подключиться
                Network.Connect( remoteServer, NETWORK_PORT ); // подключаемся к серверу
            }
        } else if ( Network.peerType == NetworkPeerType.Connecting ) { // во время подключения
            GUILayout.Label( "Trying to connect to server" ); // выводим текст
        } else { // в остальных случаях ( NetworkPeerType.Server, NetworkPeerType.Client)
            if ( GUILayout.Button( "Disconnect" ) ) {  // кнопка отключиться
                Network.Disconnect(); // отключить все клиенты либо отключиться от сервера
            }
        }
    }

    void OnFailedToConnect( NetworkConnectionError error ) {
        Debug.Log( "Failed to connect: " + error.ToString() ); // при ошибке подключения к серверу выводим саму ошибку
    }

    void OnDisconnectedFromServer( NetworkDisconnection info ) {
        if ( Network.isClient ) {
            Debug.Log( "Disconnected from server: " + info.ToString() ); // при успешном либо не успешном отключении выводим результат
        } else {
            Debug.Log( "Connections closed" ); // выводим при выключении сервера Network.Disconnect
        }
    }

    void OnConnectedToServer() {
        Debug.Log( "Connected to server" ); // выводим при успешном подключении к серверу
    }
}