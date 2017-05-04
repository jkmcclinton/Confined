using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinDistance : MonoBehaviour {

    RoomIdentifier r;
    MusicController mc;
    SceneController sc;

	// Use this for initialization
	void Start () {
        mc = FindObjectOfType<MusicController>();
        sc = FindObjectOfType<SceneController>();
        r = transform.parent.GetComponent<RoomIdentifier>();
	}

    void OnTriggerEnter(Collider coll) {
        GameObject go = coll.gameObject;
        if (!go.name.ToLower().Equals("interactspace")) return;
        if(r.music!=null)mc.song = r.music;
        sc.setRoom(r);
    }
}
