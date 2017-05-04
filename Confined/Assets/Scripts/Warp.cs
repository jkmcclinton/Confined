using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warp : MonoBehaviour {

    GameObject arrow;
    BoxCollider body;
    Evaluator evaluator;
    SceneController sc;
    SoundController sound;
    Player player;
    Door door;
    private RoomIdentifier baseRoom;
    public RoomIdentifier goal;
    public string condition;
    float loadTime;

    public enum Direction {
        North, East, South, West
    }

    public Direction direction = Direction.South;

	// Use this for initialization
	void Start () {
        body = GetComponent<BoxCollider>();
        evaluator = FindObjectOfType<Evaluator>();
        sc = FindObjectOfType<SceneController>();
        player = FindObjectOfType<Player>();
        sound = FindObjectOfType<SoundController>();
        baseRoom = GetComponentInParent<RoomIdentifier>();
        if (goal != null) {
            GameObject tmp = GameObject.Find(baseRoom.name+":"+ goal.name);
            if (tmp != null) {
                door = tmp.GetComponent<Door>();
                door.unTurn();
            } else {
                tmp = GameObject.Find(goal.name + ":" + baseRoom.name );
                if (tmp != null) door = tmp.GetComponent<Door>();
            }
        }
	}

    // Update is called once per frame
    Interactable interact; 
	void Update () {
        if (loadTime > 0) {
            loadTime -= Time.deltaTime;
            if(loadTime<=0)
                sc.loadScript(interact.scriptFile);
        } else {
            if (goal.state == RoomIdentifier.StateType.HIDDEN && !sc.analyzing) {
                if (Input.GetButtonDown("Door") && arrow != null) {
                    player.interact();
                    if (evaluator.evaluate(condition)) {
                        sound.playSound("Close", false);
                        goal.show();
                        if (door != null) {
                            Debug.Log(sc.Room.name + " [" + door.baseRoom.name + "|" + door.destRoom.name + "]");
                            if (sc.Room == door.baseRoom) door.unTurn();
                            else door.Turn();
                            door.open();
                        }
                    } else {
                        sound.playSound("Locked", false);
                        interact = transform.parent.gameObject.GetComponent<Interactable>();
                        if (interact != null) {
                            loadTime = .633f;
                        }
                    }
                }
            }
        }
    }
    
    void OnTriggerEnter(Collider collider) {
        GameObject go = collider.gameObject; Vector3 pos = transform.position;
        float w = body.size.x;
        if (!go.name.ToLower().Equals("interactspace")) return;
        if (goal.state != RoomIdentifier.StateType.HIDDEN) return;

        //player.door = this;

        Vector3 dir = Vector3.zero;
        switch (direction) {
            case Direction.North: dir = Vector3.forward; w = body.size.z;  break;
            case Direction.East: dir = Vector3.right; break;
            case Direction.South: dir = Vector3.back; w = body.size.z; break;
            case Direction.West: dir = Vector3.left; break;
        }
        w /= 2;
        if (evaluator.evaluate(condition)) {
            pos.y -= body.size.y / 2f;
            pos += w * dir;
            arrow = Instantiate(Resources.Load("Prefabs/Arrow"), pos,
                Quaternion.LookRotation(dir, Vector3.up), transform) as GameObject;
        } else {
            if (direction == Direction.West || direction == Direction.North) {
                Vector3 off = Vector3.zero;
                switch (direction) {
                    case Direction.North: off = Vector3.right; break;
                    default: off = Vector3.back; break;
                }
                pos -= .5f * dir - w*off;
            } else
                pos += w * dir;
            arrow = Instantiate(Resources.Load("Prefabs/Locked"), pos,
                Quaternion.LookRotation(dir, Vector3.up), transform) as GameObject;

        }
    }

    void OnTriggerExit(Collider collider) {
        GameObject go = collider.gameObject;
        if (!go.name.ToLower().Equals("interactspace")) return;
        if (arrow != null) Destroy(arrow);
        //player.door = null;
    }
}
