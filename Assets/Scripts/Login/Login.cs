using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MiniJSON;

public class Login : MonoBehaviour
{

    private GameObject feedback;

    /// <summary>
    /// Checks if a WWW objects has resulted in an error, and if so throws an exception to deal with it.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    /// <param name="www">The request object.</param>
    private void checkWwwStatus(string errorMessage, WWW www)
    {
        if (!string.IsNullOrEmpty(www.error))
        {
            throw new Trello.TrelloException(errorMessage + ": " + www.error);
        }
    }

    public void OnCreate()
    {
        string username = string.Empty;
        string password = string.Empty;
        string board = string.Empty;

        foreach (Transform child in this.transform)
        {
            if (child.name == "ID")
            {
                foreach (Transform kid in child.transform)
                {
                    if (kid.name == "Text")
                    {
                        username = kid.GetComponent<Text>().text;
                    }
                }
            }
            if (child.name == "PASSWORD")
            {
                foreach (Transform kid in child.transform)
                {
                    if (kid.name == "Text")
                    {
                        password = kid.GetComponent<Text>().text;
                    }
                }            
            }
            if (child.name == "Board")
            {
                foreach (Transform kid in child.transform)
                {
                    if (kid.name == "Text")
                    {
                        board = kid.GetComponent<Text>().text;
                    }
                }
            }
            if (child.name == "feedback")
            {
                feedback = child.gameObject;
            }
        }

        WWW www = new WWW(Trello.Trello.loginServerUrl + "client/findClient?" + "user_name=" + username + "&pass_word=" + password);

        // Wait for request to return
        while (!www.isDone)
        {
            checkWwwStatus("Connection to the Trello servers was not possible", www);
        }

        var dict = Json.Deserialize(www.text) as Dictionary<string, object>;

        if ((bool)dict["notFound"] == true)
        {
           feedback.GetComponent<Text>().text = "Username or Password not correct";
        }
        else {
            var dictionary = (Dictionary<string, object>)dict["data"];




            List<object> boards;
            WWW www1 = new WWW("https://api.trello.com/1/members/me" + "?" + "key=" + (string)dictionary["key"] + "&token=" + (string)dictionary["token"] + "&boards=all");

            // Wait for request to return
            while (!www1.isDone)
            {
                checkWwwStatus("Connection to the Trello servers was not possible", www1);
            }

            var dic = Json.Deserialize(www1.text) as Dictionary<string, object>;

            boards = (List<object>)dic["boards"];

            if (boards == null)
            {
                feedback.GetComponent<Text>().text = "No board for this user!";
                throw new Trello.TrelloException("You have not yet populated the list of boards, so one cannot be selected.");
            }

            for (int i = 0; i < boards.Count; i++)
            {
                var currentboard = (Dictionary<string, object>)boards[i];
                if ((string)currentboard["name"] == board)
                {
                    Trello.Trello.key = (string)dictionary["key"];
                    Trello.Trello.token = (string)dictionary["token"];
                    Trello.Trello.board = board;
                    feedback.GetComponent<Text>().text = "Loading......";
                    SceneManager.LoadScene("trello");
                    return;
                }
            }

            feedback.GetComponent<Text>().text = "Do not exist this board for given user!";































        }

    }
}
