using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Interactable : MonoBehaviour {

    /*[HideInInspector]*/ public string scriptFile;
    public bool isEnabled = true;
    public bool react;

    [HideInInspector] public bool door;

    Player player;
    Mesh mesh;
    Material outline;
    GameObject outlinedObj;
    SceneController sc;
    float intCoolDown;
    bool marking;

    void Reset() {
        scriptFile = new List<string>(Vars.SCRIPT_LIST.Keys)[0];
    }

	// Use this for initialization
	void Start () {
        player = GameObject.Find("Rin").GetComponent<Player>();
        mesh = GetComponentInChildren<MeshFilter>().mesh;
        outline = Resources.Load("Materials/Outline")
            as Material;
        sc = GameObject.FindObjectOfType<SceneController>();
        door = GetComponentInChildren<Warp>() != null;
    }
	
	// Update is called once per frame
	void Update () {
        if (!isEnabled) return;
        intCoolDown -= intCoolDown > 0 ? Time.deltaTime : 0;
        if (marking && !sc.analyzing) {
            marking = false;
            intCoolDown = .75f;
        }

        if (player.currentInteractable == null) stopHighlight();
        else if (!player.currentInteractable.Equals(this)) stopHighlight();
	}

    public void stopHighlight() {
        if(outlinedObj!=null)outlinedObj.SetActive(false);
    }

    public void mark() {
        // mark current object for interaction cooldown (.75s?)
        // remove item from current interactable, prevent it from 
        // becomming interactable until cooldown is done
        stopHighlight();
        marking = true;
    }

    void OnTriggerEnter(Collider collider) {
        if (!isEnabled) return;
        GameObject go = collider.gameObject;
        if (!go.name.ToLower().Equals("interactspace") ||
            intCoolDown>0) return;
        player.currentInteractable = this;
        //mR.material.shader = Resources.Load("Materials/Silhouette-OutlinedDiffuse")
        //    as Shader;

        if (outlinedObj != null) {
            outlinedObj.SetActive(true);
        } else {
            outlinedObj = new GameObject("Outlined Obj");
            outlinedObj.transform.parent = transform;
            outlinedObj.transform.SetPositionAndRotation(transform.position, transform.rotation);
            MeshRenderer mR = outlinedObj.AddComponent<MeshRenderer>();
            MeshFilter mF = outlinedObj.AddComponent<MeshFilter>();
            mF.mesh = mesh;
            mR.material = outline;

        }
    }

    void OnTriggerExit(Collider collider) {
        if (!isEnabled) return;
        GameObject go = collider.gameObject;
        if (!go.name.ToLower().Equals("interactspace")) return;
        stopHighlight();
        if (player.currentInteractable != null)
            if (player.currentInteractable==this)
                player.currentInteractable = null;
    }
}
