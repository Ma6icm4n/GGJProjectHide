using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class NetworkRoomManagerNew : NetworkRoomManager
{
    public override void OnRoomStopClient()
    {
        if(gameObject.scene.name == "DontDestroyOnLoad" && !string.IsNullOrEmpty(offlineScene) && SceneManager.GetActiveScene().path != offlineScene)
        {
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
        }

        base.OnRoomStopClient();
    }

    public override void OnRoomStopServer()
    {
        if (gameObject.scene.name == "DontDestroyOnLoad" && !string.IsNullOrEmpty(offlineScene) && SceneManager.GetActiveScene().path != offlineScene)
        {
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
        }

        base.OnRoomStopServer();
    }

    bool showStartButton;

    public override void OnRoomServerPlayersReady()
    {
#if UNITY_SERVER
        base.OnRoomServerPlayersReady();
#else
        showStartButton = true;
#endif
    }

    public override void OnGUI()
    {
        base.OnGUI();

        if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "Start Game"))
        {
            showStartButton = false;

            ServerChangeScene(GameplayScene);
        }
    }
}
