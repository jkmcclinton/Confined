using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomIdentifier : MonoBehaviour {

    static float hideLoc = -40;
    private float t = .1f;

    Vector3 hid, sho;
    SceneController sc;
    MusicController mc;
    //FadeObj fader;
    public AudioClip music;
    [SerializeField] StateType stateType = StateType.HIDING;
    public GroundType groundType = GroundType.Stone;

    public StateType state { get { return stateType; } }

    Shader shader;
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
        shader = Resources.Load<Shader>("Materials/Model");
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
            Color c = e.Key.material.color;
            e.Key.material.color = new Color(c.r, c.g, c.b, 0);
        }
    }

    void reappear() {
        foreach (KeyValuePair<MeshRenderer, Shader> e in cols) {
            e.Key.material.shader = e.Value;
        }
    }

    void updateLoc() {
        switch (stateType) {
            case StateType.HIDDEN:
                transform.position = hid;
                break;
            case StateType.HIDING:
                transform.position = Vector3.Lerp(transform.position, hid, t/3f);
                if (Vector3.Distance(transform.position, hid) < .02f) {
                    stateType = StateType.HIDDEN;
                    disappear();
                } else {
                    foreach (KeyValuePair<MeshRenderer, Shader> e in cols) {
                        Color c = e.Key.material.color;
                        e.Key.material.color = new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, 0, t));
                    }
                }
                break;
            case StateType.SHOWING:
                transform.position = Vector3.Lerp(transform.position, sho, t);
                if (Vector3.Distance(transform.position, sho) < .02f) {
                    stateType = StateType.SHOWN;
                    reappear();
                } else {
                    foreach (KeyValuePair<MeshRenderer, Shader> e in cols) {
                        Color c;
                            c = e.Key.material.color;
                            e.Key.material.color = new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, 1, t));
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

        //fader.fadeIn();
    }

    public void hide() {
        stateType = StateType.HIDING;
        reset();
        //fader.fadeOut();

    }

    void reset() {
        foreach (KeyValuePair<MeshRenderer, Shader> e in cols) {
            e.Key.material.shader = shader;
        }
    }

    void OnTriggerEnter(Collider coll) {
        GameObject go = coll.gameObject;
        if (!go.name.ToLower().Equals("interactspace")) return;
        sc.current = this;
        if(music!=null)mc.song = music;
        sc.setRoom(this);
    }

    void OnTriggerExit(Collider coll) {
        GameObject go = coll.gameObject;
        if (!go.name.ToLower().Equals("interactspace")) return;
        hide();
    }
}
