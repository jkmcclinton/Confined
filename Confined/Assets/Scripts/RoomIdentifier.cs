using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomIdentifier : MonoBehaviour {

    static float hideLoc = -40;
    private float speed = .2f;

    Vector3 hid, sho;
    SceneController sc;
    MusicController mc;
    //FadeObj fader;
    public AudioClip music;
    [SerializeField] StateType stateType = StateType.HIDING;
    public GroundType groundType = GroundType.Stone;

    public StateType state { get { return stateType; } }

    //Shader shader;
    int albedo;
    Dictionary<MeshRenderer, Shader> cols;

    public enum StateType {
        HIDDEN, HIDING, SHOWING, SHOWN
    }

    public enum GroundType {
        Wood, Grass, Stone
    }

    // Use this for initialization
    void Start () {
        sc = GameObject.Find("SceneController").GetComponent<SceneController>();
        mc = GameObject.Find("MusicController").GetComponent<MusicController>();
        //fader = GetComponent<FadeObj>();
        sho = transform.position ;
        hid = new Vector3(sho.x, sho.y+hideLoc, sho.z);
        cols = new Dictionary<MeshRenderer, Shader>();
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>()) {
            cols.Add(mr, mr.material.shader);
        }

        //GameObject transparent = GameObject.Find("TRANSPARENT_SHADER");
        //if(transparent!=null) shader = transparent.GetComponent<MeshRenderer>().material.shader;
        albedo = Shader.PropertyToID("_Color");
        
        if (state == StateType.HIDING || state == StateType.HIDDEN) {
            reset();
            if (state == StateType.HIDDEN)
                disappear();
        }
    }
	
	// Update is called once per frame
	void Update () {
        updateLoc();
    }

    void disappear() {
        foreach (KeyValuePair<MeshRenderer, Shader> e in cols) {
            Color c = e.Key.material.GetColor(albedo);
            e.Key.material.SetColor(albedo, new Color(c.r, c.g, c.b, 0));
            e.Key.enabled = false;
            //e.Key.material.SetFloat("_Mode", 3);
        }
    }

    void reappear() {
        foreach (KeyValuePair<MeshRenderer, Shader> e in cols) {
            e.Key.material.shader = e.Value;
            //e.Key.material.SetFloat("_Mode", 0);
        }
    }

    void updateLoc() {
        switch (stateType) {
            case StateType.HIDDEN:
                transform.position = hid;
                break;
            case StateType.HIDING:
                transform.position = Vector3.Lerp(transform.position, hid, speed);
                if (Vector3.Distance(transform.position, hid) < .02f) {
                    stateType = StateType.HIDDEN;
                    disappear();
                } else {
                    foreach (KeyValuePair<MeshRenderer, Shader> e in cols) {
                        Color c = e.Key.material.GetColor(albedo);
                        e.Key.material.SetColor(albedo, new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, 0, speed)));
                    }
                }
                break;
            case StateType.SHOWING:
                transform.position = Vector3.Lerp(transform.position, sho, speed);
                if (Vector3.Distance(transform.position, sho) < .02f) {
                    stateType = StateType.SHOWN;
                    reappear();
                } else {
                    foreach (KeyValuePair<MeshRenderer, Shader> e in cols) {
                        Color c;
                            c = e.Key.material.GetColor(albedo);
                            e.Key.material.SetColor(albedo, new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, 1, speed)));
                    } 
                }
                break;
            case StateType.SHOWN:
                transform.position = sho;
                break;
        }
    }

    public void show() {
        stateType = StateType.SHOWING;

        foreach (KeyValuePair<MeshRenderer, Shader> e in cols) {
            e.Key.enabled=true;
        }
        //fader.fadeIn();
    }

    public void hide() {
        stateType = StateType.HIDING;
        reset();
        //fader.fadeOut();

    }

    void reset() {
        //foreach (KeyValuePair<MeshRenderer, Shader> e in cols) {
            //e.Key.material.shader = shader;
        //}
    }

    void OnTriggerExit(Collider coll) {
        GameObject go = coll.gameObject;
        if (!go.name.ToLower().Equals("interactspace")) return;
        hide();
    }
}
