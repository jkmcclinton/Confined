using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour {
    GameObject player;
    Vector3 offset;
    // Use this for initialization
    void Start () {
        player = GameObject.Find("Rin");
        offset = transform.position - player.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = player.transform.position + offset;
	}
}
