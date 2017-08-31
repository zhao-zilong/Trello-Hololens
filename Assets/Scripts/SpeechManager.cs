using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.UI;
using System.Text;

public class SpeechManager : MonoBehaviour
{
    public KeywordRecognizer keywordRecognizer = null;

    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    private GameObject FocusedObject = null;
    Vector3 originalPosition;
    private bool isGoing = false;
    private bool isBacking = false;
    private bool isBack = false;
    private bool isFlipping = false;
    private bool isFlipped = false;
    private float transmissionCompletePercentage = 0f;
    private float rotationCompletePercentage = 0f; 
    private Quaternion startRotation;
    private Quaternion endRotation;

    void Update()
    {

        //called in "show detail", stop when card arrives to the position
        if (isGoing) {

            transmissionCompletePercentage += Time.deltaTime * Trello.Trello.TransitionSpeed;
            FocusedObject.transform.position = Vector3.Lerp(originalPosition , Trello.Trello.detailPosition, transmissionCompletePercentage);
            this.gameObject.GetComponent<PhotonView>().RPC("cardPosition", PhotonTargets.AllViaServer, FocusedObject.transform.position, FocusedObject.name);

            if (transmissionCompletePercentage > 1.0f)
            {
                isGoing = false;
                isBack = false;
                transmissionCompletePercentage = 0f;
            }

        }
        //called in "dismiss card", stop when card returns to the original position
        if (isBacking) {
            transmissionCompletePercentage += Time.deltaTime * Trello.Trello.TransitionSpeed;
            FocusedObject.transform.position = Vector3.Lerp(Trello.Trello.detailPosition, originalPosition, transmissionCompletePercentage);
            this.gameObject.GetComponent<PhotonView>().RPC("cardPosition", PhotonTargets.AllViaServer, FocusedObject.transform.position, FocusedObject.name);

            if (transmissionCompletePercentage > 1.0f)
            {
                isBacking = false;
                isBack = true;
                transmissionCompletePercentage = 0f;
                if (isFlipped == false)
                {
                    FocusedObject = null;
                    FocusedObject.GetComponent<CardCommands>().setCaller(null);
                }
            }
        }

        //called in "flip card", stop when card rotates 180 degrees
        if (isFlipping) {
            rotationCompletePercentage += Time.deltaTime * Trello.Trello.TransitionSpeed;
            FocusedObject.transform.rotation = Quaternion.Slerp(startRotation, endRotation, rotationCompletePercentage);
            this.gameObject.GetComponent<PhotonView>().RPC("cardRotation", PhotonTargets.AllViaServer, FocusedObject.transform.rotation, FocusedObject.name);
            Debug.Log(FocusedObject.transform.rotation);
            if (rotationCompletePercentage > 1.0f)
            {
                isFlipped = !isFlipped;
                isFlipping = false;
                rotationCompletePercentage = 0f;
                if (isBack == true) {

                    FocusedObject = null;
                    FocusedObject.GetComponent<CardCommands>().setCaller(null);
                }
            }
        }
    }



    // Initialize all key word for speech recognise system
    void Start()
    {

        keywords.Add("Quit game", () =>
        {
                Application.Quit();
            });

        keywords.Add("Detect environment", () =>
        {
            if (!SpatialMapping.Instance.MappingEnabled) {

                SpatialMapping.Instance.MappingEnabled = true;

            }
            SpatialMapping.Instance.DrawVisualMeshes = true;
        });

        keywords.Add("Adapt environment", () =>
        {
            Debug.Log("adapt environment recognise successful!");

            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("card");

            for (var i = 0; i < gameObjects.Length; i++)
            {
                Destroy(gameObjects[i]);
            }
            gameObjects = GameObject.FindGameObjectsWithTag("board");
            for (var i = 0; i < gameObjects.Length; i++)
            {
                Destroy(gameObjects[i]);
            }
            Trello.Trello trello = new Trello.Trello();
            trello.AdaptEnvironment();
        });


        keywords.Add("Refresh board", () =>
        {
            Debug.Log("Refresh speech recognise successful!");

            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("card");

            for (var i = 0; i < gameObjects.Length; i++)
            {
                Destroy(gameObjects[i]);
            }
            gameObjects = GameObject.FindGameObjectsWithTag("board");
            for (var i = 0; i < gameObjects.Length; i++)
            {
                Destroy(gameObjects[i]);
            }
            Trello.Trello trello = new Trello.Trello();
            trello.RefreshBoard();
        });

        keywords.Add("Come closer", () =>
        {
            Debug.Log("Refresh speech recognise successful!");

            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("card");

            for (var i = 0; i < gameObjects.Length; i++)
            {
                Destroy(gameObjects[i]);
            }
            gameObjects = GameObject.FindGameObjectsWithTag("board");
            for (var i = 0; i < gameObjects.Length; i++)
            {
                Destroy(gameObjects[i]);
            }
            Trello.Trello trello = new Trello.Trello();
            trello.ApproachBoard();
        });


        keywords.Add("keep away", () =>
        {
            Debug.Log("Refresh speech recognise successful!");

            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("card");

            for (var i = 0; i < gameObjects.Length; i++)
            {
                Destroy(gameObjects[i]);
            }
            gameObjects = GameObject.FindGameObjectsWithTag("board");
            for (var i = 0; i < gameObjects.Length; i++)
            {
                Destroy(gameObjects[i]);
            }
            Trello.Trello trello = new Trello.Trello();
            trello.AwayBoard();
        });
        keywords.Add("Flip card", () =>
        {
            Debug.Log("Flip card recognise successful!");
            Debug.Log(FocusedObject == null);
            if (FocusedObject != null) {
                startRotation = FocusedObject.transform.rotation;
                endRotation = FocusedObject.transform.rotation * Quaternion.Euler(0, 180, 0);
                isFlipping = true;
            }
        });


        //Show the card in a closer(to user) position
        keywords.Add("Show detail", () =>
        {
            Debug.Log("in show me detail");
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;

            RaycastHit hitInfo;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
            {

                if (hitInfo.collider.gameObject.tag == "card")
                {

                    FocusedObject = hitInfo.collider.gameObject;
                    //already owned by other user
                    if (FocusedObject.GetComponent<CardCommands>().getOwner() != null) {
                        FocusedObject = null;
                        return;
                    }
                    FocusedObject.GetComponent<CardCommands>().setCaller(this.gameObject);
                    originalPosition = FocusedObject.transform.position;
                    isGoing = true;
                }
                Debug.Log(FocusedObject.transform.position);
            }

        });

        keywords.Add("Dismiss Card", () =>
        {
            Debug.Log("Dismiss speech recognise successful!");
            if (FocusedObject != null) {
                isBacking = true;
            }
            if (isFlipped) {
                startRotation = FocusedObject.transform.rotation;
                endRotation = FocusedObject.transform.rotation * Quaternion.Euler(0, 180, 0);
                isFlipping = true;
            }
        });

        //Support to edit only description and title for now
        keywords.Add("Edit Card", () =>
        {
            Debug.Log("in show me detail");
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;

            RaycastHit hitInfo;
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {

            if (hitInfo.collider.gameObject.tag == "card")
            {
                GameObject FocusedGameObject = hitInfo.collider.gameObject;
                string title = FocusedGameObject.GetComponent<CardCommands>().title;
                string description = FocusedGameObject.GetComponent<CardCommands>().description;
                    Debug.Log(description);
                //get urls
                    MatchCollection ms = Regex.Matches(description, @"(www.+|http.+|https.)([\s]|$)$");
                    if (ms != null && ms.Count != 0)
                    {
                        string words = ms[0].Value.ToString();
                        Debug.Log(words.Split(' ')[0]);
                        Debug.Log("<link=\"CurrentLink\">" + words.Split(' ')[0] + "</link>");
                        if (description.Contains(words.Split(' ')[0]))
                        {
                            StringBuilder builder = new StringBuilder(description);
                            Debug.Log("contains substring");
                            builder.Replace(words.Split(' ')[0], "<link=\"CurrentLink\">" + words.Split(' ')[0] + "</link>");
                            description = builder.ToString();
                        }
                    }

                    foreach (Transform children in GameObject.Find("CanvasTaker").transform)
                    {
                        if (children.gameObject.name == "EditorTaker" && children.gameObject.activeSelf == false)
                        {
                            children.gameObject.SetActive(true);
                            foreach (Transform child in children.transform)
                            {
                                if (child.gameObject.name == "CardEditor")
                                {
                                    foreach (Transform enfant in child.transform)
                                    {
                                        if (enfant.gameObject.name == "CardPanel")
                                        {
                                            enfant.GetComponent<CardEditor>().card = FocusedGameObject;
                                            foreach (Transform bambino in enfant.transform)
                                            {
                                                if (bambino.gameObject.name == "Title")
                                                {

                                                    bambino.gameObject.GetComponent<InputField>().text = title;
                                                    continue;
                                                }
                                                if (bambino.gameObject.name == "Context")
                                                {
                                                    //bambino.gameObject.GetComponent<InputField>().text = description;
                                                    Debug.Log(description);
                                                    bambino.gameObject.GetComponent<TMPro.TMP_InputField>().text = description;
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        });

        keywords.Add("Take Card", () =>
            {

                GameObject focusObject = this.gameObject.GetComponent<GazeGestureManager>().FocusedObject;
 

                if (focusObject != null)
                    {
                    Debug.Log(focusObject.GetComponent<CardCommands>().moving);
                        if (focusObject.GetComponent<CardCommands>().moving == true)
                        {
                            return;
                        }
                        Debug.Log(focusObject.tag == "card");
                        if (focusObject.GetComponent<CardCommands>().moving == false
                        && focusObject.tag == "card")
                        {
                        focusObject.GetComponent<CardCommands>().setCaller(this.gameObject);
                        }
                        this.gameObject.GetComponent<PhotonView>().RPC("Onselect", PhotonTargets.Others, focusObject.name);
                        focusObject.SendMessageUpwards("OnSelect");
                    }
            });

            keywords.Add("Drop Card", () =>
            {
                GameObject focusObject = this.gameObject.GetComponent<GazeGestureManager>().FocusedObject;

                if (focusObject != null)
                {
                    
                    if (focusObject.GetComponent<CardCommands>().moving == false
                        || focusObject.GetComponent<CardCommands>().getOwner() != this.gameObject)
                    {
                        return;
                    }
                    this.gameObject.GetComponent<PhotonView>().RPC("Onselect", PhotonTargets.Others, focusObject.name);
                    focusObject.SendMessage("OnSelect");
                }
            });

            keywords.Add("Create Card", () =>
            {
                Debug.Log("create card speech recognise successful!");
                //GameObject instance = GameObject.Instantiate(Resources.Load("CanvasTaker")) as GameObject;
                foreach (Transform child in GameObject.Find("CanvasTaker").transform) {
                    if (child.gameObject.name == "CreatorTaker" && child.gameObject.activeSelf == false)
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            });


        // Tell the KeywordRecognizer about our keywords.
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        // Register a callback for the KeywordRecognizer and start recognizing!
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }
}