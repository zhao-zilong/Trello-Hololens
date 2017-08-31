using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR.WSA.Input;

public class GazeGestureManager : MonoBehaviour
{
    public GazeGestureManager Instance { get; private set; }

    // Represents the hologram that is currently being gazed at.
    public GameObject FocusedObject;

    public Text text;

    GestureRecognizer recognizer;

    void Start()
    {
        Instance = this;

        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            // Send an OnSelect message to the focused object and its ancestors.
            if (FocusedObject != null && FocusedObject.tag == "card")
            {

                //if the card is moving and the controller is not local player
                if (FocusedObject.GetComponent<CardCommands>().moving == true
                && FocusedObject.GetComponent<CardCommands>().getOwner() != this.gameObject) {
                    return;
                }
               
                //if the card is not moving
                if (FocusedObject.GetComponent<CardCommands>().moving == false
                && FocusedObject.tag == "card") {
                    FocusedObject.GetComponent<CardCommands>().setCaller(this.gameObject);
                }

                this.gameObject.GetComponent<PhotonView>().RPC("Onselect", PhotonTargets.Others, FocusedObject.name);            
                FocusedObject.SendMessageUpwards("OnSelect");
                
            }
        };
        recognizer.StartCapturingGestures();
    }

    // Update is called once per frame
    void Update()
    {
        // Figure out which hologram is focused this frame.
        GameObject oldFocusObject = FocusedObject;

        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            // If the raycast hit a hologram, use that as the focused object.
            FocusedObject = hitInfo.collider.gameObject;
            if (FocusedObject.tag == "card") {
                text.text = FocusedObject.GetComponent<CardCommands>().title;
            }
        }
        else
        {
            // If the raycast did not hit a hologram, clear the focused object.
            FocusedObject = null;
        }

        // If the focused object changed this frame,
        // start detecting fresh gestures again.
        if (FocusedObject != oldFocusObject)
        {
            recognizer.CancelGestures();
            recognizer.StartCapturingGestures();
        }
    }
}
