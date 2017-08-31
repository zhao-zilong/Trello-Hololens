using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Attach this script to card prefab, called when taking a card, Update() make sure the card follows the cursur
/// </summary>

public class TapToPlaceHolocard : MonoBehaviour
{
    public bool placing = false;

    // Called by GazeGestureManager when the user performs a Select gesture
    void OnSelect()
    {
        // On each Select gesture, toggle whether the user is in placing mode.
        placing = !placing;
        Debug.Log("onselect: " + placing);
        // If the user is in placing mode, display the spatial mapping mesh.
        //if (placing)
        //{
        //    SpatialMapping.Instance.DrawVisualMeshes = true;
        //}
        //// If the user is not in placing mode, hide the spatial mapping mesh.
        //else
        //{
        //    SpatialMapping.Instance.DrawVisualMeshes = false;
        //}

    }

    // Update is called once per frame
    void Update()
    {
        // If the user is in placing mode,
        // update the placement to match the user's gaze.

        if (placing)
        {
            //  Debug.Log("update placing is true");

            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;

            RaycastHit hitInfo;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo,
                100.0f, SpatialMapping.PhysicsRaycastMask))
            {
                // Move this object to
                // where the raycast hit the Spatial Mapping mesh.

                //Debug.Log("hitinfo "+hitInfo.point);

                Vector3 pos = hitInfo.point;
                //pos.z = Trello.Trello.CARDZ;
                pos.z = pos.z - 0.005f;
                this.transform.position = pos;
                if (this.gameObject.GetComponent<CardCommands>().player != null)
                {
                    //Debug.Log("RPC called in card: " + this.name);
                    this.gameObject.GetComponent<CardCommands>().player.GetComponent<PhotonView>().RPC("cardPosition", PhotonTargets.Others, this.transform.position, this.name);
                }
                // Rotate this object to face the user.
                //Quaternion toQuat = Camera.main.transform.localRotation;
                //toQuat.x = 0;
                //toQuat.z = 0;
                //this.transform.rotation = toQuat;
            }
        }
    }


}