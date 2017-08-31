using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLoader : MonoBehaviour {

    // Use this for initialization
    IEnumerator Start()
    {

        var trello = new Trello.Trello(Trello.Trello.key, Trello.Trello.token);

        // Async, do not block
        // yield return trello.populateBoards();
        trello.setCurrentBoard(Trello.Trello.board);
        trello.populateLabels();
        // Async, do not block
        yield return trello.populateLists();
        //trello.setCurrentList("Todo");


        yield return trello.populateUsers();
        yield return trello.populateCards();
        SpatialMapping.Instance.DrawVisualMeshes = false;
    }


}
