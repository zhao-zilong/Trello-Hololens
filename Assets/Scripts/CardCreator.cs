using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Trello;
using UnityEngine.Windows.Speech;

public class CardCreator : MonoBehaviour
{

    //To test the button Submit, will delete soon
    public void OnTest() {

        TrelloCard card = new TrelloCard();
        Trello.Trello trello = new Trello.Trello();


        card.idList = trello.getListIdByName("TODO");
        card.name = "test";
        card.desc = "test description";
        card.pos = "top";
        card.due = "";
        card.idLabel = "";
        trello.uploadCard(card);
        trello.reLoadCards(trello.getListIdByName("TODO"), "red");



    }
    //Called when click on Submit button in creating card board
    public void OnCreate()
    {
        //Debug.Log("called in oncreate!");
        TrelloCard card = new TrelloCard();
        Trello.Trello trello = new Trello.Trello();
        string idList = null;
        string title = null;
        string description = null;
        string due = null;
        string color = null;
        foreach (Transform child in this.transform)
        {
            if (child.name == "Title")
            {
                foreach (Transform kid in child.transform)
                {
                    if (kid.name == "Text")
                    {
                        title = kid.GetComponent<Text>().text;
                    }
                }
                child.gameObject.GetComponent<InputField>().text = string.Empty;
            }
            if (child.name == "Context")
            {
                foreach (Transform kid in child.transform)
                {
                    if (kid.name == "Text")
                    {
                        description = kid.GetComponent<Text>().text;
                    }
                }
                child.gameObject.GetComponent<InputField>().text = string.Empty;
            }
            if (child.name == "Due")
            {
                foreach (Transform kid in child.transform)
                {
                    if (kid.name == "Text")
                    {
                        due = kid.GetComponent<Text>().text;
                    }
                }
                child.gameObject.GetComponent<InputField>().text = string.Empty;
            }
            if (child.name == "ListName")
            {
                foreach (Transform kid in child.transform)
                {
                    if (kid.name == "Text")
                    {
                        idList = trello.getListIdByName(kid.GetComponent<Text>().text);
                    }
                }
                child.gameObject.GetComponent<InputField>().text = string.Empty;
            }
            if (child.name == "Color")
            {
                //Debug.Log("check in color");
                foreach (Transform kid in child.transform)
                {
                    if (kid.name == "red")
                    {
                        if (kid.GetComponent<Toggle>().isOn)
                        {
                            color = "red";
                        }
                    }
                    if (kid.name == "green")
                    {
                        if (kid.GetComponent<Toggle>().isOn)
                        {
                            color = "green";
                        }
                    }
                    if (kid.name == "yellow")
                    {
                        if (kid.GetComponent<Toggle>().isOn)
                        {
                            color = "yellow";
                        }
                    }
                    if (kid.name == "orange")
                    {
                        if (kid.GetComponent<Toggle>().isOn)
                        {
                            color = "orange";
                        }
                    }
                    if (kid.name == "purple")
                    {
                        if (kid.GetComponent<Toggle>().isOn)
                        {
                            color = "purple";
                        }
                    }
                    if (kid.name == "blue")
                    {
                        if (kid.GetComponent<Toggle>().isOn)
                        {
                            color = "blue";
                        }
                    }
                }
            }

        }


        if (title == null || title == "") { return; }
        //if the idList does not exist, upload to the first list of board
        if (idList == null) {
            if (Trello.Trello.lists.Count == 0) { return; }
            else {
                var listInfo = (Dictionary<string, object>)Trello.Trello.lists[0];
                idList = (string)listInfo["id"];
            }
        }
        card.idList = idList;
        card.name = title;
        card.desc = description;
        card.pos = "top";
        string idLabel = null;
        color = color.ToLower();
        if (Trello.Trello.labels.ContainsKey(color)) {
            Trello.Trello.labels.TryGetValue(color, out idLabel);
            card.idLabel = idLabel;
        }
        if (due != null && due != "") { card.due = due; }
        

        bool complete = false;
        bool isIdExist = Trello.Trello.isIdExist(idList);
        if (isIdExist)
        {
            complete = trello.uploadCard(card);
            if (complete)
            {
                foreach (Transform child in this.transform)
                {
                    if (child.name == "Feedback")
                    {
                        child.GetComponent<Text>().text = "upload completed!";
                        //call RPC to notify other users to reload card
                        //             CardCeator       CrreatorTaker    CanvasTaker      CapsulePlayer 
                        this.transform.parent.transform.parent.transform.parent.transform.parent.GetComponent<PhotonView>().RPC("LoadNewCard", PhotonTargets.Others, idList , color);
                        trello.reLoadCards(idList, color);
                    }
                }
            }
            else
            {
                foreach (Transform child in this.transform)
                {
                    if (child.name == "Feedback")
                    {
                        child.GetComponent<Text>().text = "upload failed";
                    }
                }
            }
        }
        else {
            foreach (Transform child in this.transform)
            {
                if (child.name == "Feedback")
                {
                    child.GetComponent<Text>().text = "Can not find this list by the name";
                }
            }
        }
        Debug.Log("cardCreator"+PhraseRecognitionSystem.Status);

        PhraseRecognitionSystem.Shutdown();
        Debug.Log("restart keywordRecognizer");
            
        PhraseRecognitionSystem.Restart();

    }
    public void Close() {
        Debug.Log("get clicked");

        foreach (Transform child in this.transform)
        {
            if (child.name == "Feedback")
            {
                child.GetComponent<Text>().text = "";
                break;
            }
        }

        foreach (Transform child in GameObject.Find("CanvasTaker").transform)
        {
            if (child.gameObject.name == "CreatorTaker" && child.gameObject.activeSelf == true)
            {
                child.gameObject.SetActive(false);
            }
        }
        PhraseRecognitionSystem.Shutdown();
        Debug.Log("restart keywordRecognizer");
            
        PhraseRecognitionSystem.Restart();

    }
}
