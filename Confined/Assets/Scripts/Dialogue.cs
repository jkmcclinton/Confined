using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour {
    
    private string page;
    /*[HideInInspector]*/ public int si;
    Text txtElmt;
    RectTransform panel;
    [HideInInspector] public StateType stateType = StateType.HIDDEN;
    Image im;
    static Vector2 hid = new Vector2(0, 300), show = new Vector2 (0, 0);
    Color goalCol, txt_goalCol;
    Player player;
    public int speed = 2;
    [HideInInspector] public float skipCoolDown;
    SoundController sound;
    bool played;

    public enum StateType { HIDDEN, HIDING, SHOWING, SHOWN }

    // Use this for initialization
    void Start () {
        player = GameObject.FindObjectOfType<Player>();
        txtElmt = gameObject.GetComponent<Text>();
        panel = (RectTransform) txtElmt.transform.parent;
        im = panel.gameObject.GetComponent<Image>();
        goalCol = im.color; txt_goalCol = txtElmt.color;
        sound = FindObjectOfType<SoundController>();
    }
	
	// Update is called once per frame
	void Update () {
        skipCoolDown -= skipCoolDown > 0 ? Time.deltaTime : 0;
        Vector2 goal = stateType == StateType.SHOWN || stateType == StateType.SHOWING ?
            show : hid;
        float min = .04f;
        switch (stateType) {
            case StateType.HIDDEN:
                txtElmt.text = "";
                im.color = new Color(goalCol.r, goalCol.g, goalCol.b, 0);
                im.color = new Color(txt_goalCol.r, txt_goalCol.g, txt_goalCol.b, 0);
                panel.anchoredPosition = goal;
                played = false;
                break;
            case StateType.HIDING:
                im.color = Color.Lerp(im.color, new Color(goalCol.r, goalCol.g, goalCol.b, 0),.5f);
                txtElmt.color = Color.Lerp(txtElmt.color, new Color(txt_goalCol.r, txt_goalCol.g, txt_goalCol.b, 0), .5f);
                panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, goal, .5f);
                if (Vector2.Distance(panel.anchoredPosition, goal) < Vector2.Distance(show, hid) / 2
                    && !played) {
                    sound.playSound("Close", false);
                    played = true;
                }
                if (Vector2.Distance(panel.anchoredPosition, goal) < min)
                    stateType = StateType.HIDDEN;
                break;
            case StateType.SHOWING:
                im.color = Color.Lerp(im.color, goalCol, .5f);
                txtElmt.color = Color.Lerp(txtElmt.color, txt_goalCol, .5f);
                panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, goal, .5f);
                if (Vector2.Distance(panel.anchoredPosition, goal) < min)
                    stateType = StateType.SHOWN;
                break;
            case StateType.SHOWN:
                im.color = goalCol;
                txtElmt.color = txt_goalCol;
                panel.anchoredPosition = goal;

                if (si < page.Length) {
                    si +=/*(int) Mathf.Lerp(si, page.Length + 1, 1)*/speed;
                    if (si > page.Length) si = page.Length;
                    player.playSound("Text");
                    txtElmt.text = page.Substring(0, si);
                } 
                break;
        }
	}

    public void fill() {
        si = page.Length;
        skipCoolDown = .5f;
        txtElmt.text = page;
    }

    public void startPage(string newPage) {
        txtElmt.text = "";
        stateType = StateType.SHOWING;
        si = 0;
        page = newPage.Replace("`",",");
    }

    public void hidePage() {
        stateType = StateType.HIDING;
        page = null;
    }

    public string getPage() { return page; }
}
