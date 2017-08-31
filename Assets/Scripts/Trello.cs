using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using MiniJSON;
using System;


namespace Trello
{
	public class Trello {
		
		public static string token = "PUT YOUR TOKEN HERE";
		public static string key = "PUT YOUR KEY HERE";
        public static string board = "PUT YOUR BOARD NAME HERE";
		public static List<object> lists;
        public static List<object> cards;
        public static Dictionary<string, string> labels = new Dictionary<string, string>();
        public static Dictionary<string, List<string>> list_cards = new Dictionary<string, List<string>>(); 
        public static Dictionary<string, string> users = new Dictionary<string, string>();
        public static  string memberBaseUrl = "https://api.trello.com/1/members/me";
		public static  string boardBaseUrl = "https://api.trello.com/1/boards/";
		public static  string cardBaseUrl = "https://api.trello.com/1/cards/";
        public static string listBaseUrl = "https://api.trello.com/1/lists/";
		
        public static string loginServerUrl = "PUT IP OF SERVER ON AZURE HERE";
        public static string currentBoardId = "";

        //for card
        public static Vector3 startPosition = new Vector3(-2.8f, 0.8f, 2f);
        public static Vector3 detailPosition = new Vector3(0, 0f, 1f);

        //for board
        public static Vector3 boardStartPosition = new Vector3(-2.5f, -0.4f, 2f);
        public static int LISTLONGER = 10;
        public static float CARDGAP = 0.15f+0.01f;
        public static float LISTGAP = 0.9f+0.05f;
        public static float LISTWIDTH = 0.9f;
        public static float CARDZ = 1.95f;
        public static float TransitionSpeed = 1.0f;

        public Trello(string key, string token)
		{
            Trello.key = key;
            Trello.token = token;
            
		}

        public Trello() { }

        public static void clear() {

            token = null;
            key = null;
            lists = null;
            cards = null;
            labels = null;
            users = null;
            list_cards = null;

        }
		/// <summary>
		/// Checks if a WWW objects has resulted in an error, and if so throws an exception to deal with it.
		/// </summary>
		/// <param name="errorMessage">Error message.</param>
		/// <param name="www">The request object.</param>
		private void checkWwwStatus(string errorMessage, WWW www)
		{
			if (!string.IsNullOrEmpty(www.error))
			{
				throw new TrelloException(errorMessage + ": " + www.error);
			}
		}
		
		/// <summary>
		/// Sets the current board to search for lists in.
		/// </summary>
		/// <param name="name">Name of the board we're after.</param>
		public void setCurrentBoard(string name)
		{

            List<object> boards;
            WWW www = new WWW(memberBaseUrl + "?" + "key=" + key + "&token=" + token + "&boards=all");

            // Wait for request to return
            while (!www.isDone)
            {
                checkWwwStatus("Connection to the Trello servers was not possible", www);
            }

            var dict = Json.Deserialize(www.text) as Dictionary<string, object>;

            boards = (List<object>)dict["boards"];

            if (boards == null)
			{
				throw new TrelloException("You have not yet populated the list of boards, so one cannot be selected.");
			}
			
			for (int i = 0; i < boards.Count; i++)
			{
				var board = (Dictionary<string, object>)boards[i];
				if ( (string)board["name"] == name)
				{
					currentBoardId = (string)board["id"];
                    return;
				}
			}
			
			currentBoardId = "";
            throw new TrelloException("No such board found.");
		}

        /// <summary>
        /// Populate the labels for the current board, used when color the card.
        /// </summary>
        public void populateLabels()
        {

            WWW www = new WWW(boardBaseUrl + currentBoardId + "/labels" + "?" + "key=" + key + "&token=" + token);
            // Wait for request to return
            Debug.Log(www.url);
            while (!www.isDone)
            {
                checkWwwStatus("Connection to the Trello servers was not possible", www);
            }
            var dict = Json.Deserialize(www.text) as List<object>;
            if (labels == null)
            {
                labels = new Dictionary<string, string>();
            }
            for (int i = 0; i < dict.Count; i++)
            {
                var label = (Dictionary<string, object>)dict[i];
                //Debug.Log("color: " + (string)label["color"] + " id: " + (string)label["id"]);
                labels.Add((string)label["color"], (string)label["id"]);
            }

        }


        /// <summary>
        /// Populate the lists for the current board, these are cached for easy card uploading later.
        /// </summary>
        /// <returns>A parsed JSON list of lists.</returns>
        public List<object> populateLists()
		{
			lists = null;
			
			if (currentBoardId == "")
			{
				throw new TrelloException("Cannot retreive the lists, you have not selected a board yet.");
			}
			
			WWW www = new WWW(boardBaseUrl + currentBoardId + "/lists"+ "?" + "key=" + key + "&token=" + token);

			
			// Wait for request to return
			while (!www.isDone)
			{
				checkWwwStatus("Connection to the Trello servers was not possible", www);
			}
			
			var dict = Json.Deserialize(www.text) as List<object>;
			
			lists = dict;

            for (int i = 0; i < lists.Count; i++)
            {
                var list = (Dictionary<string, object>)lists[i];
                GameObject instance = GameObject.Instantiate(Resources.Load("Board")) as GameObject;
                instance.name = (string)list["name"];
                foreach (Transform child in instance.transform)
                {
                    foreach (Transform node in child)
                    {
                        if (node.name == "Text")
                        {
                            node.GetComponent<Text>().text = (string)list["name"];
                        }
                    }
                }
                Vector3 pos = boardStartPosition;
                pos.x += i * LISTGAP;
                instance.transform.position = pos;
                instance.GetComponent<BoardInformation>().idList = (string)list["id"];
                List<string> temp = new List<string>();
                if(list_cards == null){
                    list_cards = new Dictionary<string, List<string>>();
                }                
                list_cards.Add((string)list["id"], temp);
                
            }
            return lists;
		}

        //Test if given id of list is exsiting.
        public static bool isIdExist(string id) {
            for (int i = 0; i < lists.Count; i++)
            {
                var list = (Dictionary<string, object>)lists[i];
                if ((string)list["id"] == id)
                {
                    return true;
                }
            }
            return false;
        }

        public string getListNameById(string id) {

            for (int i = 0; i < lists.Count; i++)
            {
                var list = (Dictionary<string, object>)lists[i];
                if ((string)list["id"] == id)
                {
                    return (string)list["name"];
                }
            }

            throw new TrelloException("No such list found.");
        }

        public string getListIdByName(string name)
        {

            for (int i = 0; i < lists.Count; i++)
            {
                var list = (Dictionary<string, object>)lists[i];
                if ((string)list["name"] == name)
                {
                    return (string)list["id"];
                }
            }

            return null;
        }

        /// <summary>
        /// Populate the all cards for the current board, these are cached for easy card uploading later.
        /// Load all cards' information dans environment
        /// </summary>
        public List<object> populateCards() {

            cards = null;

            if (currentBoardId == "")
            {
                throw new TrelloException("Cannot retreive the cards, you have not selected a board yet.");
            }

            WWW www = new WWW(boardBaseUrl + currentBoardId + "/cards/" + "?" + "key=" + key + "&token=" + token);


            // Wait for request to return
            while (!www.isDone)
            {
                checkWwwStatus("Connection to the Trello servers was not possible", www);
            }

            var dict = Json.Deserialize(www.text) as List<object>;

            //Debug.Log(www.text);

            cards = dict;


            //start position of the cards in each boards
            Vector3 pos = new Vector3(0, 0, 0);
            int[] counter = new int[lists.Count];
            for (int i = 0; i < cards.Count; i++)
            {
                var card = (Dictionary<string, object>)cards[i];

                pos = startPosition;

                List<string> temp;
                list_cards.TryGetValue((string)card["idList"],out temp);
                temp.Add((string)card["id"]);
                
                int numberOfList = GetNumberOfListByListId((string)card["idList"]);                           
                pos.x = pos.x + numberOfList * LISTGAP;             
                pos.y = pos.y - counter[numberOfList] * CARDGAP;            
                counter[numberOfList]++;
                
                if (counter[numberOfList] > LISTLONGER)
                {
                    float coeff = counter[numberOfList] / LISTLONGER;
                    if (counter[numberOfList] % LISTLONGER == 0) {
                         coeff = coeff - 1.0f;
                    }
                    coeff = (float)Math.Floor(coeff);
                    pos.y += LISTLONGER * CARDGAP * coeff;
                    pos.x += CARDGAP * coeff;
                }


                GameObject instance = enableCardInstance(card);
                instance.transform.position = pos;


                //Load the color correspondant to the cards
                List<object> labels = (List<object>)card["labels"];
                if (labels.Count > 0)
                {
                    var label = (Dictionary<string, object>)labels[0];
                    enableInstanceColor((string)label["color"], instance);
                }

            }

            return cards;

        }


        /// <summary>
        /// Load all members with their id
        /// </summary>
        public string populateUsers()
        {
            WWW www = new WWW(boardBaseUrl + currentBoardId + "/members/" + "?" + "key=" + key + "&token=" + token);


            // Wait for request to return
            while (!www.isDone)
            {
                checkWwwStatus("Connection to the Trello servers was not possible", www);
            }

                var raw = Json.Deserialize(www.text) as List<object>;
                for (int i = 0; i < raw.Count; i++)
                {
                    var member = (Dictionary<string, object>)raw[i];
                    //Debug.Log((string)member["id"] + "  " + (string)member["fullName"]);
                    if (users == null) { users = new Dictionary<string, string>(); }
                    users.Add((string)member["id"], (string)member["fullName"]);
                }
                   // Debug.Log("populateUsers");
                    return "Load Users' information complete!";

        }

        /// <summary>
        /// Get username by id
        /// </summary>
        public string getUserNameById(string id) {
            String name;
            if (users.TryGetValue(id, out name))
            {

                return name;
            }
            else {

                throw new TrelloException("Can not find username by id provided");
            }
       
        }

        /// <summary>
        /// return which list the card belongs to
        /// </summary>
        public static int GetNumberOfListByListId(string id) {

            for (int i = 0; i < lists.Count; i++)
            {
                var list = (Dictionary<string, object>)lists[i];
                if ((string)list["id"] == id)
                {
                    return i;
                }
            }
            throw new TrelloException("No such list found in GetNumberofListByListId()");

        }



        
        /// <summary>
        /// reload the cards of idList when add a new card
        /// </summary>
        /// 
        public void reLoadCards(string idList, string color)
        {
            WWW www = new WWW(listBaseUrl + idList + "/cards/" + "?" + "key=" + key + "&token=" + token);
            // Wait for request to return
            while (!www.isDone)
            {
                checkWwwStatus("Connection to the Trello servers was not possible", www);
            }

            var dict = Json.Deserialize(www.text) as List<object>;
            List<string> temporary;
            list_cards.TryGetValue(idList, out temporary);

            if (dict.Count > 0)
            {
                cards.Add(dict[0]);
                var card = (Dictionary<string, object>)dict[0];
                GameObject instance = enableCardInstance(card);
                instance.transform.position = startPosition;
                enableInstanceColor(color, instance);
                temporary.Insert(0, (string)card["id"]);

            }
          
            reLocateCards(idList);
        }

        //called in SpeechManager.cs to refresh the board and list
        public void AdaptEnvironment()
        {

            lists = null;
            cards = null;
            labels = null;
            users = null;
            list_cards = null;
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;

            RaycastHit hitInfo;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo,
                100.0f, SpatialMapping.PhysicsRaycastMask))
            {
                Vector3 pos = hitInfo.point;

                startPosition.z = pos.z - 0.05f;
                boardStartPosition.z = pos.z - 0.05f;
                CARDZ = pos.z - 0.05f - 0.05f;
            }
            populateLabels();
            populateLists();
            populateUsers();
            populateCards();
            SpatialMapping.Instance.DrawVisualMeshes = false;
            //to be tested
            SpatialMapping.Instance.MappingEnabled = false;
        }



        //called in SpeechManager.cs to refresh the board and list
        public void RefreshBoard() {

            lists = null;
            cards = null;
            labels = null;
            users = null;
            list_cards = null;
            populateLabels();
            populateLists();
            populateUsers();
            populateCards();
        }
        public void ApproachBoard()
        {

            lists = null;
            cards = null;
            labels = null;
            users = null;
            list_cards = null;
            startPosition.z -= 0.5f;
            boardStartPosition.z -= 0.5f;
            CARDZ -= 0.5f;
            populateLabels();
            populateLists();
            populateUsers();
            populateCards();
        }
        public void AwayBoard()
        {

            lists = null;
            cards = null;
            labels = null;
            users = null;
            list_cards = null;
            startPosition.z += 0.5f;
            boardStartPosition.z += 0.5f;
            CARDZ += 0.5f;
            populateLabels();
            populateLists();
            populateUsers();
            populateCards();
        }


        public bool uploadCard(TrelloCard card)
        {
            WWWForm post = new WWWForm();
            post.AddField("name", card.name);
            post.AddField("desc", card.desc);
            post.AddField("pos", card.pos);
            post.AddField("due", card.due);
            post.AddField("idList", card.idList);
            post.AddField("idLabels", card.idLabel);


            WWW www = new WWW(cardBaseUrl + "?" + "key=" + key + "&token=" + token, post);

            // Wait for request to return
            while (!www.isDone)
            {
                checkWwwStatus("Could not upload Trello card", www);
            }

            Debug.Log("upload complete!");
            return true;
        }

        // relocate the cards' order of given list(idList)
        public static void reLocateCards(string idList)
        {
            if (idList == string.Empty) {
                return;
            }
            List<string> cards;
            list_cards.TryGetValue(idList, out cards);
            int numberOfList = GetNumberOfListByListId(idList);
            
            //Debug.Log("list longer in trello: " + cards.Count);
            for (int i = 0; i < cards.Count; i++) {
                //Debug.Log("turn: " + i);
                Vector3 pos = startPosition;
                pos.x = pos.x + numberOfList * LISTGAP;
                pos.y = pos.y - i * CARDGAP;
                

                if (i >= LISTLONGER)
                {
                    float coeff = i / LISTLONGER;

                    coeff = (float)Math.Floor(coeff);
                    pos.y += LISTLONGER * CARDGAP * coeff;
                    pos.x += CARDGAP * coeff;
                }
                //Debug.Log("cardId: " + cards[i] + "position " +pos);
                GameObject card = GameObject.Find(cards[i]);
                if (card.GetComponent<CardCommands>().moving != true)
                {
                    card.transform.position = pos;
                }
                //        player.GetComponent<PhotonView>().RPC("cardPosition", PhotonTargets.AllViaServer, card.transform.position, card.name);
            }
            Debug.Log("finish in relocatecards");
        }

        //called in CardCommands.cs, serve to predict the card position in the original list
        public static void preLocateOriginalList(string idList , int indexOriginal ,int indexCurrent) {
            List<string> cards;
            list_cards.TryGetValue(idList, out cards);
            int numberOfList = GetNumberOfListByListId(idList);
            int j = 0;
            //Debug.Log("list longer in trello: " + cards.Count);
            for (int i = 0; i < cards.Count; i++)
            {
                
                //skip the current carrying card
                if (i == indexOriginal) {
                    //Debug.Log("indexOriginal: "+i);
                    continue;
                }
                if (indexOriginal < indexCurrent) {
                    //Debug.Log("biggerCurrent");
                    if (i == indexOriginal + 1) {
                        j = -1;
                    }
                    if (i == indexCurrent) {
                        j = 0;
                    }
                }
                if (indexOriginal > indexCurrent)
                {
                    if (i == indexOriginal + 1)
                    {
                        j = 0;
                    }
                    if (i == indexCurrent)
                    {
                        j = 1;
                    }
                }
                Vector3 pos = startPosition;
                pos.x = pos.x + numberOfList * LISTGAP;
                pos.y = pos.y - (i+j) * CARDGAP;


                if ((i + j) >= LISTLONGER)
                {
                    float coeff = (i + j) / LISTLONGER;

                    coeff = (float)Math.Floor(coeff);
                    pos.y += LISTLONGER * CARDGAP * coeff;
                    pos.x += CARDGAP * coeff;
                }
                GameObject card = GameObject.Find(cards[i]);
                if (card.GetComponent<CardCommands>().moving != true)
                {
                    card.transform.position = pos;
                }
            }
           // Debug.Log("finish in preLocateOriginalList");

        }

        //called in CardCommands.cs, serve to predict the card position in a new list
        public static void preLocateNewList(string idList, int index)
        {
            List<string> cards;
            list_cards.TryGetValue(idList, out cards);
            int numberOfList = GetNumberOfListByListId(idList);
            int j = 0;
            //Debug.Log("index " + index);
            for (int i = 0; i < cards.Count; i++)
            {
                //skip the current carrying card
                if (i == index)
                {
                    j = 1;
                }

                Vector3 pos = startPosition;
                pos.x = pos.x + numberOfList * LISTGAP;
                pos.y = pos.y - (i + j) * CARDGAP;


                if ((i + j) >= LISTLONGER)
                {
                    float coeff = (i + j) / LISTLONGER;

                    coeff = (float)Math.Floor(coeff);
                    pos.y += LISTLONGER * CARDGAP * coeff;
                    pos.x += CARDGAP * coeff;
                }
                //Debug.Log("cardId: " + cards[i] + "position " +pos);
                GameObject card = GameObject.Find(cards[i]);
                if (card.GetComponent<CardCommands>().moving != true)
                {
                    card.transform.position = pos;
                }

            }
          //  Debug.Log("finish in preLocateNewList");

        }

        // instantiate the card
        public GameObject enableCardInstance(Dictionary<string, object> card) {

            GameObject instance = GameObject.Instantiate(Resources.Load("card")) as GameObject;
            instance.name = (string)card["id"];
            instance.GetComponent<CardCommands>().id = (string)card["id"];
            instance.GetComponent<CardCommands>().title = (string)card["name"];
            instance.GetComponent<CardCommands>().description = (string)card["desc"];
            instance.GetComponent<CardCommands>().due = (string)card["due"];
            instance.GetComponent<CardCommands>().idList = (string)card["idList"];
            instance.GetComponent<CardCommands>().position = float.Parse(card["pos"].ToString());

            //load members of the card
            List<string> memberName = new List<string>();
            var memberId = (List<object>)card["idMembers"];
            for (int j = 0; j < memberId.Count; j++)
            {
                memberName.Add(getUserNameById(memberId[j].ToString()));
            }
            instance.GetComponent<CardCommands>().members = memberName;

            foreach (Transform child in instance.transform)
            {
                foreach (Transform node in child)
                {
                    if (node.name == "Title")
                    {
                        node.GetComponent<Text>().text = (string)card["name"];
                    }
                    if (node.name == "Context")
                    {
                        node.GetComponent<Text>().text = (string)card["desc"];
                    }
                    if (node.name == "Due")
                    {
                        node.GetComponent<Text>().text = (string)card["due"];
                    }
                    if (node.name == "Member")
                    {
                        string namelist = string.Empty;
                        if (memberName.Count != 0)
                        {
                            for (int i = 0; i < memberName.Count; i++)
                            {
                                if (i == 0)
                                {
                                    namelist = memberName[i];
                                }
                                else {
                                    namelist = namelist + "," + memberName[i];
                                }
                            }
                        }
                        node.GetComponent<Text>().text = namelist;
                    }
                }
            }
            return instance;
        }


        //load material correspondant to the card
        public void enableInstanceColor(string color, GameObject instance) {

            switch (color)
            {
                case "red":
                    instance.GetComponent<Renderer>().material = Resources.Load("MaterialsCards/cardred") as Material;
                    break;
                case "green":
                    instance.GetComponent<Renderer>().material = Resources.Load("MaterialsCards/cardgreen") as Material;
                    break;
                case "orange":
                    instance.GetComponent<Renderer>().material = Resources.Load("MaterialsCards/cardorange") as Material;
                    break;
                case "yellow":
                    instance.GetComponent<Renderer>().material = Resources.Load("MaterialsCards/cardyellow") as Material;
                    break;
                case "purple":
                    instance.GetComponent<Renderer>().material = Resources.Load("MaterialsCards/cardpurple") as Material;
                    break;
                case "blue":
                    instance.GetComponent<Renderer>().material = Resources.Load("MaterialsCards/cardblue") as Material;
                    break;
                default:
                    break;

            }

        }



    }
}