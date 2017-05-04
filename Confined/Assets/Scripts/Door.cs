using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    bool turned, opened;
    Animator anim;
    GameObject model;
    SoundController sound;

    public RoomIdentifier baseRoom;
    public RoomIdentifier destRoom;

	// Use this for initialization
	void Start () {
        string[] rooms = name.Split(':');

        GameObject t = GameObject.Find(rooms[0]);
        if (t != null) baseRoom = t.GetComponent<RoomIdentifier>();

        t = GameObject.Find(rooms[1]);
        if (t != null) destRoom = t.GetComponent<RoomIdentifier>();

        anim = GetComponent<Animator>();
        Transform tmp = transform.FindChild("Model");
        if (tmp != null) model = tmp.gameObject;
        sound = FindObjectOfType<SoundController>();
        //Debug.Log(name + ": " + baseRoom);
	}
	
	// Update is called once per frame
	void Update () {
        // basically if we havent vreated the room yet in the scene
        if (destRoom != null) {
            if (baseRoom.state == RoomIdentifier.StateType.HIDING ||
        destRoom.state == RoomIdentifier.StateType.HIDING)
                close();
        } else 
            if (baseRoom.state == RoomIdentifier.StateType.HIDING)
                close();
	}

    public void Turn() {
        if (anim == null) return;
        anim.SetBool("Turned", turned = true);
    }

    public void unTurn() {
        if (anim==null) return;
        anim.SetBool("Turned", turned = false);
    }

    public void open() {
        opened = true;
        anim.ResetTrigger("Close");
        anim.SetTrigger("Open");
    }

    public void close() {
        opened = false;
        anim.ResetTrigger("Open");
        anim.SetTrigger("Close");
    }

    void play() {
        if(opened) sound.playSound("Door", false);
        else sound.playSound("Step", false);
    }


}
