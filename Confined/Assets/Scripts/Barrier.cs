using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour {

    public string condition;
    List<BoxCollider> barriers;
    bool disabled;
    Evaluator evaluator;

	// Use this for initialization
	void Start () {
        evaluator = GameObject.FindObjectOfType<Evaluator>();
        barriers = new List<BoxCollider>(GetComponents<BoxCollider>());
        barriers.AddRange(GetComponentsInChildren<BoxCollider>());
        List<BoxCollider> tmp = new List<BoxCollider>();
        foreach (BoxCollider b in barriers) 
            if (b.isTrigger) tmp.Add(b);
        foreach (BoxCollider b in tmp)
            if (barriers.Contains(b)) barriers.Remove(b);

	}
	
	// Update is called once per frame
	void Update () {
        if (!disabled) {
            if (evaluator.evaluate(condition)) {
                disabled = true;
                foreach (BoxCollider b in barriers) {
                    b.enabled = false;
                }
            }
        }
	}
}
