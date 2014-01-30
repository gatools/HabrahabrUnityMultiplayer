using UnityEngine;
using System.Collections;

[RequireComponent( typeof( Rigidbody ) )] // в данном примере нам понадобиться риджедбоди

public class PlayerControls : MonoBehaviour {
    /* для интерполяции */
    private float lastSynchronizationTime; // последнее время синхронизации
    private float syncDelay = 0f; // дельта между текущем временем и последней синхронизацией
    private float syncTime = 0f; // время синхронизации

    private Vector3 syncStartPosition = Vector3.zero; //начальная позиция интерполяции 
    private Vector3 syncEndPosition = Vector3.zero; // конечная позиция интерполяции

    private Quaternion syncStartRotation = Quaternion.identity; // начальный поворот интерполяции
    private Quaternion syncEndRotation = Quaternion.identity; // конечный поворот интерполяции

    private NetworkView netView; // компонент NetworkView

    private string myName = ""; // наше имя (так, для примера :) мы его не используем)
    public string MyName { get { return myName; } } // паблик доступ к имени
    public float power = 20f;

    void Awake() {
        netView = gameObject.AddComponent( typeof( NetworkView ) ) as NetworkView; // добавляем компонент NetworkView нашему игровому обьекту
        netView.viewID = Network.AllocateViewID(); // присваеваем уникальный индификатор в сети
        netView.observed = this; // указиваем этот скрипт (компонент) для синхронизации
        netView.stateSynchronization = NetworkStateSynchronization.Unreliable; // нам подходит способ быстрой передачи с потерями так как наше передвижение интерполируется
        lastSynchronizationTime = Time.time; // последнее время синхронизации
    }

    void FixedUpdate() {
        if ( netView.isMine ) { // если обькт принадлежит нам то мы им управляем иначе делаем интерполяцию движения
            float inputX = Input.GetAxis( "Horizontal" );
            float inputY = Input.GetAxis( "Vertical" );
			if ( inputX != 0.0f ) {
                rigidbody.AddTorque( Vector3.forward * -inputX * power, ForceMode.Impulse );
            }
            if ( inputY != 0.0f ) {
                rigidbody.AddTorque( Vector3.right * inputY * power, ForceMode.Impulse );
            }
        } else {
			syncTime += Time.fixedDeltaTime;
            rigidbody.position = Vector3.Lerp( syncStartPosition, syncEndPosition, syncTime / syncDelay ); // интерполяция перемещения
            rigidbody.rotation = Quaternion.Lerp( syncStartRotation, syncEndRotation, syncTime / syncDelay ); // интерполяция поворота
		}
	}

    void OnSerializeNetworkView( BitStream stream, NetworkMessageInfo info ) {
        Vector3 syncPosition = Vector3.zero; // для синхронизации позиции
        Vector3 syncVelocity = Vector3.zero; // для синхронизации действующей силы
        Quaternion syncRotation = Quaternion.identity; // для синхронизации поворота

        if ( stream.isWriting ) { // если отправляем в сеть то считываем данные обьекта и отправляем
            syncPosition = rigidbody.position;
            stream.Serialize( ref syncPosition );

            syncPosition = rigidbody.velocity;
            stream.Serialize( ref syncVelocity );

            syncRotation = rigidbody.rotation;
            stream.Serialize( ref syncRotation );
        } else { // иначе считываем из сети
            stream.Serialize( ref syncPosition );
            stream.Serialize( ref syncVelocity );
            stream.Serialize( ref syncRotation );

            syncTime = 0f; // сбрасываем время синхронизации
            syncDelay = Time.time - lastSynchronizationTime; // получаем дельту придыдущей синхронизации
            lastSynchronizationTime = Time.time; // записываем новое время последней синхронизации

            syncEndPosition = syncPosition + syncVelocity * syncDelay; // конечная точка в которую двигаеться обьект
            syncStartPosition = rigidbody.position; // начальная точка равна текущей позиции

            syncEndRotation = syncRotation; // конечный поворот
            syncStartRotation = rigidbody.rotation; // начальный поворот
        }
    }

    void SetPlayerName( string name ) {
        myName = name; // устанавливаем имя игрока
    }
}