using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechBubble : MonoBehaviour {

    Player player;
    Interactable owner;


	// Use this for initialization
	void Start () {
        player = GameObject.Find("Rin").GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log(transform.GetChild(0));
        if (player.currentInteractable != owner) Destroy(gameObject);
         
	}

    public void setOwner(Interactable i) { owner = i; }
}
