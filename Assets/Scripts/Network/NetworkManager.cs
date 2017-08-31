using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour {

    const string VERSION = "v0.0.1";
    public string roomName = "Holocard";
    public string Prefab = "CapsulePlayer";
    public GameObject spawnPoint;
    List<GameObject> players = new List<GameObject>();
    public static int collaborating = 0;

	void Start () {
        PhotonNetwork.ConnectUsingSettings(VERSION);
        Debug.Log("here");
    }

    //callback function if we joined lobby
    void OnJoinedLobby() {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = false;
        roomOptions.MaxPlayers = 4;
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);

    }

    //callback function if we joined room
    void OnJoinedRoom() {

        Debug.Log("load");
        GameObject player = PhotonNetwork.Instantiate(Prefab, spawnPoint.transform.position, spawnPoint.transform.rotation, 0);
        GameObject.Find("_Manager").GetComponent<NetworkManager>().players.Add(player);
    }

}
