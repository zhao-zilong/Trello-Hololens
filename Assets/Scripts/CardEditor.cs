using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class CardEditor : MonoBehaviour
{

    public GameObject card;
    public void OnEdit()
    {
        string title = string.Empty;
        string description = string.Empty;
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
                //         child.gameObject.GetComponent<InputField>().text = string.Empty;
            }
            if (child.name == "Context")
            {

                description = child.gameObject.GetComponent<TMPro.TMP_InputField>().text;
                Debug.Log("description: " + description);
                //foreach (Transform kid in child.transform)
                //{
                //    if (kid.name == "Text")
                //    {

                //    }
                //}
                //         child.gameObject.GetComponent<InputField>().text = string.Empty;
            }

        }
        //card.GetComponent<CardCommands>().title = title;
        //card.GetComponent<CardCommands>().description = description;
        //card.GetComponent<CardCommands>().updateCardName();
        //card.GetComponent<CardCommands>().updateCardDesc();
        MatchCollection ms = Regex.Matches(description, @"(www.+|http.+|https.)([\s]|$)$");
        if (ms != null && ms.Count != 0)
        {
            string words = ms[0].Value.ToString();
            Debug.Log(words.Split('<')[0]);
            if (description.Contains("<link=\"CurrentLink\">" + words.Split('<')[0] + "</link>"))
            {
                StringBuilder builder = new StringBuilder(description);
                Debug.Log("contains substring in onEdit");
                builder.Replace("<link=\"CurrentLink\">" + words.Split('<')[0] + "</link>" , words.Split('<')[0]);
                description = builder.ToString();
            }
        }



        card.GetComponent<CardCommands>().title = title;
        card.GetComponent<CardCommands>().description = description;
        bool updatecard = card.GetComponent<CardCommands>().updateCard();


        foreach (Transform child in this.transform)
        {
            if (child.name == "Feedback")
            {
                if (updatecard)
                {
                    child.GetComponent<Text>().text = "Update complete!";
                    //call RPC to notify other users to reload card
                    //             CardCeator       CrreatorTaker    CanvasTaker      CapsulePlayer 
                    this.transform.parent.transform.parent.transform.parent.transform.parent.GetComponent<PhotonView>().RPC("updateCard", PhotonTargets.Others, card.name, title, description);

                }
                else
                {
                    child.GetComponent<Text>().text = "Get error, update failed!";
                }
            }
        }

        card.GetComponent<CardCommands>().reloadCard();


    }
    public void OnClose()
    {
        foreach (Transform child in this.transform)
        {

            if (child.name == "Feedback")
            {
                child.GetComponent<Text>().text = string.Empty;
                continue;
            }
        }
        GameObject.Find("EditorTaker").SetActive(false);
        PhraseRecognitionSystem.Shutdown();
        Debug.Log("restart keywordRecognizer in cardeditor");
        PhraseRecognitionSystem.Restart();
    }
}
