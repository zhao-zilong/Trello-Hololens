using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Attach this script to player, it will initialize the script for local controller, and exclude the scripts for remote controller
/// </summary>



public class NetworkPlayer : Photon.MonoBehaviour {

    public GameObject myCamera;
    public GazeGestureManager gazeGestureManager;
    public SpeechManager speechManager;
    public GameObject body;
    public GameObject CanvasTaker;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

	void Start () {
        if (photonView.isMine)
        {
            speechManager.enabled = true;
            this.lastPosition = this.transform.position;
            this.lastRotation = this.transform.rotation;

        }
        else {
            myCamera.SetActive(false);
            gazeGestureManager.enabled = false;
            CanvasTaker.SetActive(false);
            
        }
	}


    void Update()
    {


        if (photonView.isMine)
        {
            if (Camera.main.transform.rotation != lastRotation)
            {
                body.transform.rotation = Camera.main.transform.rotation;
                lastRotation = Camera.main.transform.rotation;

            }
            if (Camera.main.transform.position != lastPosition)
            {
                body.transform.position = Camera.main.transform.position;
                lastPosition = Camera.main.transform.position;

            }
        }


    }


    [PunRPC]
    void cardPosition(Vector3 newPos, string name)
    {
        //Debug.Log("called in rpc cardposition");
        Vector3 pos = new Vector3(newPos.x, newPos.y, Trello.Trello.startPosition.z-0.05f);
        GameObject.Find(name).transform.position = pos;
    }

    [PunRPC]
    void cardRotation(Quaternion newPos, string name)
    {
        //Debug.Log("called in rpc cardposition");
        
        GameObject.Find(name).transform.rotation = newPos;
    }

    [PunRPC]
    void Onselect(string name)
    {
        //Debug.Log("called in rpc cardposition");
        
        GameObject.Find(name).GetComponent<CardCommands>().OnSelect();
    }

    [PunRPC]
    void LoadNewCard(string idList, string color)
    {
        //Debug.Log("called in rpc loadnewcard");
        Trello.Trello trello = new Trello.Trello();
        trello.reLoadCards(idList, color);
    }

    [PunRPC]
    void updateCard(string name, string title, string description)
    {
        //Debug.Log("called in rpc update");
        GameObject.Find(name).GetComponent<CardCommands>().title = title;
        GameObject.Find(name).GetComponent<CardCommands>().description = description;
        GameObject.Find(name).GetComponent<CardCommands>().reloadCard();
    }

}
