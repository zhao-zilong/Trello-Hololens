using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;


/// <summary>
/// Attach to prefab "card", contains functions to maintain the correct position in the environment
/// </summary>



public class CardCommands : MonoBehaviour
{
    Vector3 originalPosition;
    Vector3 lastPosition;
    private string lastIdList = string.Empty;

    //add set get function to all variables
    public string title;
    public string id;
    public string description;
    public string due;
    public string idList;
    public float position;
    public List<string> members = new List<string>();
    public bool moving = false;
    public GameObject player = null;

    void Start()
    {
        // Grab the original local position of the card when the app starts.
        originalPosition = this.transform.localPosition;
    }

    void Update()
    {
        if (moving && NetworkManager.collaborating<2)
        {
            float x = this.transform.position.x;
            for (int i = 0; i < Trello.Trello.lists.Count; i++)
            {
                var list = (Dictionary<string, object>)Trello.Trello.lists[i];
                string name = (string)list["name"];
                GameObject board = GameObject.Find(name);
                //the card has to be put in a board which is not the board before to trigger a movement
                if (x >= board.transform.position.x - 0.5f * Trello.Trello.LISTWIDTH && x <= board.transform.position.x + 0.5f * Trello.Trello.LISTWIDTH
                    && (lastPosition.x > board.transform.position.x + 0.5f * Trello.Trello.LISTWIDTH || lastPosition.x < board.transform.position.x - 0.5f * Trello.Trello.LISTWIDTH))
                {
                   // Debug.Log("move the card");
                                      
                    if (lastIdList != (string)list["id"]) {

                        if (lastIdList == this.idList)
                        {
                            List<string> temp;
                            Trello.Trello.list_cards.TryGetValue(idList, out temp);
                            Trello.Trello.preLocateOriginalList(this.idList, temp.IndexOf(id), temp.Count);
                        }
                        else
                        {
                            Trello.Trello.reLocateCards(lastIdList);
                        }
                        lastIdList = (string)list["id"];
                    }
                    preSortList(id, (string)list["id"]);
 

                    break;
                }
                if (x >= board.transform.position.x - 0.5f * Trello.Trello.LISTWIDTH && x <= board.transform.position.x + 0.5f * Trello.Trello.LISTWIDTH
                 && (lastPosition.x >= board.transform.position.x - 0.5f * Trello.Trello.LISTWIDTH || lastPosition.x <= board.transform.position.x + 0.5f * Trello.Trello.LISTWIDTH))
                {
                    //Debug.Log("update the order of cards in a list");
                    if (lastIdList != this.idList) {
                        Trello.Trello.reLocateCards(lastIdList);
                        lastIdList = this.idList;
                    }
                    preSortLocalList();
                    break;
                }
            }

        }
    }

    //Used to set the gameobject who control the card, in order to call RPC function attached to the gameobject
    public void setCaller(GameObject player)
    {
        this.player = player;
    }

    public GameObject getOwner()
    {
        return this.player;
    }

    /// <summary>
    /// Checks if a WWW objects has resulted in an error, and if so throws an exception to deal with it.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    /// <param name="www">The request object.</param>
    private void checkWwwStatus(string errorMessage, UnityWebRequest www)
    {
        if (!string.IsNullOrEmpty(www.error))
        {
            throw new Trello.TrelloException(errorMessage + ": " + www.error);
        }
    }

    // Called by GazeGestureManager when the user performs a Select gesture
    public void OnSelect()
    {
        Debug.Log("in select");


        moving = !moving;
        if (moving == true)
        {
            NetworkManager.collaborating++;
        }
        else {
            NetworkManager.collaborating--;
        }
        if (moving == false)
        {
            float x = this.transform.position.x;
            for (int i = 0; i < Trello.Trello.lists.Count; i++)
            {
                var list = (Dictionary<string, object>)Trello.Trello.lists[i];
                string name = (string)list["name"];
                GameObject board = GameObject.Find(name);
                //the card has to be put in a board which is not the board before to trigger a movement
                if (x >= board.transform.position.x - 0.5f * Trello.Trello.LISTWIDTH && x <= board.transform.position.x + 0.5f * Trello.Trello.LISTWIDTH
                    && (lastPosition.x > board.transform.position.x + 0.5f * Trello.Trello.LISTWIDTH || lastPosition.x < board.transform.position.x - 0.5f * Trello.Trello.LISTWIDTH))
                {
                    Debug.Log("move the card");

                    reSortList(id, this.idList, (string)list["id"]);
                    //need information of card's position
                    moveCard(id, (string)list["id"]);
                    updateCardPos();
                    this.idList = (string)list["id"];

                    break;
                }
                if (x >= board.transform.position.x - 0.5f * Trello.Trello.LISTWIDTH && x <= board.transform.position.x + 0.5f * Trello.Trello.LISTWIDTH
                 && (lastPosition.x >= board.transform.position.x - 0.5f * Trello.Trello.LISTWIDTH || lastPosition.x <= board.transform.position.x + 0.5f * Trello.Trello.LISTWIDTH))
                {
                    Debug.Log("update the order of cards in a list");
                    reSortList();
                    updateCardPos();
                    break;
                }
                
            }

            this.player = null;
        }
        lastPosition = this.transform.position;

    }



    //called when we drop a card in a new list
    public string moveCard(string idCard, string idList)
    {

        string url = Trello.Trello.cardBaseUrl + idCard + "/idList?value=" + idList + "&key=" + Trello.Trello.key + "&token=" + Trello.Trello.token;
        UnityWebRequest www = UnityWebRequest.Put(url, "move a card");
        www.Send();
        if (www.isError)
        {
            return www.error;
        }
        else
        {
            return "Upload complete!";
        }

    }


    //Predict the drop position of a card while we are taking the card outside original list and moving around
    public void preSortList(string cardId, string listIdAfter)
    {

        List<string> temp;
        Trello.Trello.list_cards.TryGetValue(listIdAfter, out temp);
        GameObject lastCard = GameObject.Find(temp[temp.Count - 1]);
        //three condition, in the left, between the cards, in the right

        //1.card moved to right of all cards or end of list in this list
        if (this.transform.position.x > lastCard.transform.position.x + Trello.Trello.CARDGAP * 0.5
            ||
            (this.transform.position.y < lastCard.transform.position.y
            && this.transform.position.x >= lastCard.transform.position.x - Trello.Trello.CARDGAP * 0.5
            && this.transform.position.x < lastCard.transform.position.x + Trello.Trello.CARDGAP * 0.5)
            )
        {
            //Debug.Log("1.card moved to right of all cards or end of list in this list");

            Trello.Trello.reLocateCards(listIdAfter);
            return;
        }


        for (int i = 0; i < temp.Count; i++)
        {
            Vector3 pos = GameObject.Find(temp[i]).transform.position;
            Vector3 cardPos = this.transform.position;
            //2.left of first line
            if (i < Trello.Trello.LISTLONGER)
            {

                if (cardPos.y > pos.y && cardPos.x < pos.x + Trello.Trello.CARDGAP * 0.5)
                {
                    Debug.Log("1.left of first line");
                    Trello.Trello.preLocateNewList(listIdAfter, i);
                    return;
                }
                

                if (temp.Count < Trello.Trello.LISTLONGER && this.transform.position.y < GameObject.Find(temp[temp.Count - 1]).transform.position.y
                    && this.transform.position.x < GameObject.Find(temp[temp.Count - 1]).transform.position.x + Trello.Trello.CARDGAP * 0.5)
                {
                    Debug.Log("1.left of first line and end of list");
                    Trello.Trello.reLocateCards(listIdAfter);
                    return;
                }
            }
            //3. between the cards
            if (cardPos.y > pos.y && cardPos.x >= pos.x - Trello.Trello.CARDGAP * 0.5 && cardPos.x < pos.x + Trello.Trello.CARDGAP * 0.5)
            {
            //    Debug.Log("3. between the cards");

                Trello.Trello.preLocateNewList(listIdAfter, i);
                return;
            }

        }

        //4. to the bottom of board where there is no card left to compare
        int numberOfLine = temp.Count / Trello.Trello.LISTLONGER;
        for (int i = 0; i < numberOfLine; i++) {
            Vector3 pos = GameObject.Find(temp[i*Trello.Trello.LISTLONGER]).transform.position;
            if (this.transform.position.x >= pos.x - Trello.Trello.CARDGAP * 0.5 && this.transform.position.x < pos.x + Trello.Trello.CARDGAP * 0.5) {
                Debug.Log("end of list: " + i);
                Trello.Trello.preLocateNewList(listIdAfter, (i+1) * Trello.Trello.LISTLONGER - 1);
                return;
            }
        }

    }



    //called when we are changing the positions of card in one list
    public void preSortLocalList()
    {
        List<string> temp;
        Trello.Trello.list_cards.TryGetValue(idList, out temp);

        GameObject lastCard = GameObject.Find(temp[temp.Count - 1]);

        //three condition, in the left, between the cards, in the right

        //1.card moved to right of all cards or end of list in this list
        if (this.transform.position.x > lastCard.transform.position.x + Trello.Trello.CARDGAP * 0.5
            ||
            (this.transform.position.y < lastCard.transform.position.y
            && this.transform.position.x >= lastCard.transform.position.x - Trello.Trello.CARDGAP * 0.5
            && this.transform.position.x < lastCard.transform.position.x + Trello.Trello.CARDGAP * 0.5)
            )
        {
            Debug.Log("in one list, to the right of all cards");
            Trello.Trello.preLocateOriginalList(this.idList, temp.IndexOf(id), temp.Count);
            return;
        }


        for (int i = 0; i < temp.Count; i++)
        {
            Vector3 pos = GameObject.Find(temp[i]).transform.position;
            Vector3 cardPos = this.transform.position;
            if (id == temp[i])
            {
                continue;
            }
            //2.left of first line
            if (i < Trello.Trello.LISTLONGER)
            {

                if (cardPos.y > pos.y && cardPos.x < pos.x + Trello.Trello.CARDGAP * 0.5)
                {
                    Debug.Log("2.left of first line");

                    //if (temp.IndexOf(id) == (Trello.Trello.LISTLONGER - 1))
                    //{
                    //    Debug.Log("2.end of first line");
                    //    Trello.Trello.preLocateOriginalList(this.idList, temp.IndexOf(id), temp.IndexOf(id));
                    //}
                    //else
                    //{


                    Debug.Log("2.not end of first line");
                        Trello.Trello.preLocateOriginalList(this.idList, temp.IndexOf(id), i);
                    //}
                    return;
                }

                if (temp.Count < Trello.Trello.LISTLONGER && this.transform.position.y < GameObject.Find(temp[temp.Count - 1]).transform.position.y
                    && this.transform.position.x < GameObject.Find(temp[temp.Count - 1]).transform.position.x + Trello.Trello.CARDGAP * 0.5)
                {
                    Debug.Log("1.left of first line and end of list");
                    Trello.Trello.preLocateOriginalList(this.idList, temp.IndexOf(id), temp.Count);
                    return;

                }
            }
            //3. between the cards
            if (cardPos.y > pos.y && cardPos.x >= pos.x - Trello.Trello.CARDGAP * 0.5 && cardPos.x < pos.x + Trello.Trello.CARDGAP * 0.5)
            {
                Debug.Log("3. between the cards");
                if (temp.IndexOf(id) == (i - 1))
                {
                    Trello.Trello.preLocateOriginalList(this.idList, temp.IndexOf(id), temp.IndexOf(id));
                }
                else
                {
                    Trello.Trello.preLocateOriginalList(this.idList, temp.IndexOf(id), i);
                }
                return;
            }

        }
        //4. to the bottom of board where there is no card left to compare
        int numberOfLine = temp.Count / Trello.Trello.LISTLONGER;
        int indexOfListOfCurrent = temp.IndexOf(id) / Trello.Trello.LISTLONGER;
        Vector3 position;
        for (int i = 0; i < numberOfLine; i++)
        {
            if (i > indexOfListOfCurrent)
            {
                position = GameObject.Find(temp[i * Trello.Trello.LISTLONGER + 1]).transform.position;
            }
            else {
                position = GameObject.Find(temp[i * Trello.Trello.LISTLONGER]).transform.position;
            }
            if (this.transform.position.x >= position.x - Trello.Trello.CARDGAP * 0.5 && this.transform.position.x < position.x + Trello.Trello.CARDGAP * 0.5)
            {
                Debug.Log("end of list: " + i);
                if (i >= indexOfListOfCurrent)
                {
                    Trello.Trello.preLocateOriginalList(this.idList, temp.IndexOf(id), (i + 1) * Trello.Trello.LISTLONGER);
                }
                else {
                    Trello.Trello.preLocateOriginalList(this.idList, temp.IndexOf(id), (i + 1) * Trello.Trello.LISTLONGER - 1);
                }

                return;
            }
        }

    }




    //After dropping the card, find the position of the new card in the new list,  then pass the new list to relocatecards()
    public void reSortList(string cardId, string listIdBefore, string listIdAfter)
    {

        List<string> temp;
        Trello.Trello.list_cards.TryGetValue(listIdBefore, out temp);
        temp.Remove(cardId);
        Trello.Trello.reLocateCards(listIdBefore);
        Trello.Trello.list_cards.TryGetValue(listIdAfter, out temp);

        //move to an empty list
        if (temp.Count == 0)
        {
            Vector3 pos = Trello.Trello.startPosition;
            int numberOfList = Trello.Trello.GetNumberOfListByListId(listIdAfter);
            pos.x = pos.x + numberOfList * Trello.Trello.LISTGAP;
            this.transform.position = pos;
            //player.GetComponent<PhotonView>().RPC("cardPosition", PhotonTargets.AllViaServer, this.transform.position, this.name);
            temp.Add(cardId);
            return;
        }

        //three condition, in the left, between the cards, in the right

        //1.card moved to right of all cards or end of list in this list
        if (this.transform.position.x > GameObject.Find(temp[temp.Count - 1]).transform.position.x + Trello.Trello.CARDGAP * 0.5
            ||
            (this.transform.position.y < GameObject.Find(temp[temp.Count - 1]).transform.position.y
            && this.transform.position.x >= GameObject.Find(temp[temp.Count - 1]).transform.position.x - Trello.Trello.CARDGAP * 0.5
            && this.transform.position.x < GameObject.Find(temp[temp.Count - 1]).transform.position.x + Trello.Trello.CARDGAP * 0.5)
            )
        {
            Debug.Log("1.card moved to right of all cards or end of list in this list");
            this.position = GameObject.Find(temp[temp.Count - 1]).GetComponent<CardCommands>().position + 65535;
            temp.Add(cardId);
            Trello.Trello.reLocateCards(listIdAfter);
            return;
        }


        for (int i = 0; i < temp.Count; i++)
        {
            Vector3 pos = GameObject.Find(temp[i]).transform.position;
            Vector3 cardPos = this.transform.position;
            //2.left of first line
            if (i < Trello.Trello.LISTLONGER)
            {

                if (cardPos.y > pos.y && cardPos.x < pos.x + Trello.Trello.CARDGAP * 0.5)
                {
                    Debug.Log("2.left of first line");
                    //become half position of first card
                    if (i == 0) { this.position = GameObject.Find(temp[i]).GetComponent<CardCommands>().position * 0.5f; }
                    else
                    {
                        this.position = (GameObject.Find(temp[i]).GetComponent<CardCommands>().position + GameObject.Find(temp[i - 1]).GetComponent<CardCommands>().position) * 0.5f;
                    }
                    temp.Insert(i, cardId);
                    Trello.Trello.reLocateCards(listIdAfter);
                    return;
                }

                if (temp.Count < Trello.Trello.LISTLONGER && this.transform.position.y < GameObject.Find(temp[temp.Count - 1]).transform.position.y
                    && this.transform.position.x < GameObject.Find(temp[temp.Count - 1]).transform.position.x + Trello.Trello.CARDGAP * 0.5)
                {
                    Debug.Log("2.left of first line and end of list");
                    this.position = GameObject.Find(temp[temp.Count - 1]).GetComponent<CardCommands>().position + 65535;
                    temp.Add(cardId);
                    Trello.Trello.reLocateCards(listIdAfter);
                    return;
                }
            }
            //3. between the cards
            if (cardPos.y > pos.y && cardPos.x >= pos.x - Trello.Trello.CARDGAP * 0.5 && cardPos.x < pos.x + Trello.Trello.CARDGAP * 0.5)
            {
                Debug.Log("3. between the cards");
                this.position = (GameObject.Find(temp[i]).GetComponent<CardCommands>().position + GameObject.Find(temp[i - 1]).GetComponent<CardCommands>().position) * 0.5f;
                temp.Insert(i, cardId);
                Trello.Trello.reLocateCards(listIdAfter);
                return;
            }

        }


    }


    //After dropping the card, called when change the positions of cards in one list
    public void reSortList()
    {
        List<string> temp;
        Trello.Trello.list_cards.TryGetValue(idList, out temp);



        //three condition, in the left, between the cards, in the right

        //1.card moved to right of all cards or end of list in this list
        if (this.transform.position.x > GameObject.Find(temp[temp.Count - 1]).transform.position.x + Trello.Trello.CARDGAP * 0.5
            ||
            (this.transform.position.y < GameObject.Find(temp[temp.Count - 1]).transform.position.y
            && this.transform.position.x >= GameObject.Find(temp[temp.Count - 1]).transform.position.x - Trello.Trello.CARDGAP * 0.5
            && this.transform.position.x < GameObject.Find(temp[temp.Count - 1]).transform.position.x + Trello.Trello.CARDGAP * 0.5)
            )
        {

            if (id == temp[temp.Count - 1])
            {
                Trello.Trello.reLocateCards(idList);
                return;
            }
            Debug.Log("1.card moved to right of all cards or end of list in this list");
            temp.Remove(id);
            this.position = GameObject.Find(temp[temp.Count - 1]).GetComponent<CardCommands>().position + 65535;
            temp.Add(id);
            Trello.Trello.reLocateCards(idList);
            return;
        }


        for (int i = 0; i < temp.Count; i++)
        {
            Vector3 pos = GameObject.Find(temp[i]).transform.position;
            Vector3 cardPos = this.transform.position;
            //2.left of first line
            if (i < Trello.Trello.LISTLONGER)
            {

                if (cardPos.y > pos.y && cardPos.x < pos.x + Trello.Trello.CARDGAP * 0.5)
                {
                    Debug.Log("2.left of first line");
                    if (id == temp[i])
                    {
                        Trello.Trello.reLocateCards(idList);
                        return;
                    }
                    //become half position of first card
                    if (i == 0) { this.position = GameObject.Find(temp[i]).GetComponent<CardCommands>().position * 0.5f; }
                    else
                    {
                        this.position = (GameObject.Find(temp[i]).GetComponent<CardCommands>().position + GameObject.Find(temp[i - 1]).GetComponent<CardCommands>().position) * 0.5f;
                    }

                    int index = temp.IndexOf(id);
                    temp.Remove(id);
                    if (i < index)
                    {
                        temp.Insert(i, id);
                    }
                    else
                    {

                        temp.Insert(i - 1, id);
                    }
                    Trello.Trello.reLocateCards(idList);
                    return;
                }

                if (temp.Count < Trello.Trello.LISTLONGER && this.transform.position.y < GameObject.Find(temp[temp.Count - 1]).transform.position.y
                    && this.transform.position.x < GameObject.Find(temp[temp.Count - 1]).transform.position.x + Trello.Trello.CARDGAP * 0.5)
                {
                    Debug.Log("2.left of first line and end of list");
                    if (id == temp[temp.Count - 1])
                    {
                        Trello.Trello.reLocateCards(idList);
                        return;
                    }

                    temp.Remove(id);
                    this.position = GameObject.Find(temp[temp.Count - 1]).GetComponent<CardCommands>().position + 65535;
                    temp.Add(id);
                    Trello.Trello.reLocateCards(idList);
                    return;

                }
            }
            //3. between the cards
            if (cardPos.y > pos.y && cardPos.x >= pos.x - Trello.Trello.CARDGAP * 0.5 && cardPos.x < pos.x + Trello.Trello.CARDGAP * 0.5)
            {
                Debug.Log("3. between the cards");
                if (id == temp[i])
                {
                    Trello.Trello.reLocateCards(idList);
                    return;
                }
                this.position = (GameObject.Find(temp[i]).GetComponent<CardCommands>().position + GameObject.Find(temp[i - 1]).GetComponent<CardCommands>().position) * 0.5f;
                int index = temp.IndexOf(id);
                temp.Remove(id);
                if (i < index)
                {
                    temp.Insert(i, id);
                }
                else
                {

                    temp.Insert(i - 1, id);
                }
                Trello.Trello.reLocateCards(idList);
                return;
            }

        }
        Trello.Trello.reLocateCards(idList);


    }


    public void updateCardPos()
    {

        string url = Trello.Trello.cardBaseUrl + id + "/pos?value=" + position + "&key=" + Trello.Trello.key + "&token=" + Trello.Trello.token;
        UnityWebRequest www = UnityWebRequest.Put(url, "relocate the card");
        www.Send();
        if (www.isError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Update card postion success!");
        }

    }

    public bool updateCardName()
    {

        string url = Trello.Trello.cardBaseUrl + id + "/name?value=" + WWW.EscapeURL(title) + "&key=" + Trello.Trello.key + "&token=" + Trello.Trello.token;
        
        UnityWebRequest www = UnityWebRequest.Put(url, "update card's name");
        Debug.Log(www.url);
        www.Send();
        while (!www.isDone)
        {
            checkWwwStatus("Connection to the Trello servers was not possible", www);
        }

        if (www.isError)
        {
            Debug.Log(www.error);
            return false;
        }
        else
        {
            Debug.Log("Update card name success!");
            return true;
        }
    }

    public bool updateCardDesc()
    {

        string url = Trello.Trello.cardBaseUrl + id + "/desc?value=" + WWW.EscapeURL(description) + "&key=" + Trello.Trello.key + "&token=" + Trello.Trello.token;
        UnityWebRequest www = UnityWebRequest.Put(url, "update card's desc");
        Debug.Log(www.url);
        www.Send();
        if (www.isError)
        {
            Debug.Log(www.error);
            return false;
        }
        else
        {
            Debug.Log("Update card descripion success!");
            return true;
        }
    }

    // update card's information on Trello
    public bool updateCard()
    {

        string url = Trello.Trello.cardBaseUrl + id + "/?desc=" + WWW.EscapeURL(description) + "&name=" + WWW.EscapeURL(title) + "&key=" + Trello.Trello.key + "&token=" + Trello.Trello.token;
        UnityWebRequest www = UnityWebRequest.Put(url, "update card");
        Debug.Log("url: " + url);
        www.Send();
        if (www.isError)
        {
                            Debug.Log(www.error);
            return false;
        }
        else
        {
                           Debug.Log("Upload complete!");
            return true;
        }

    }


    public void reloadCard() {

        foreach (Transform child in this.transform)
        {
            foreach (Transform node in child)
            {
                if (node.name == "Title")
                {
                    node.GetComponent<Text>().text = title;
                }
                if (node.name == "Context")
                {
                    node.GetComponent<Text>().text = description;
                }
            }
        }


    }

}