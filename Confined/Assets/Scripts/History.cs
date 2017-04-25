using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class History : MonoBehaviour {

    private Dictionary<string, bool> flags;
    private Dictionary<string, int> vars;
    
    void Start() {
        flags = new Dictionary<string, bool>();
        vars = new Dictionary<string, int>();
    }

   public bool flag(string f) {
        if (flags.ContainsKey(f))
            return flags[f];
        else
            return false;
    }

    public void setFlag(string n, bool v) {
        if (flags.ContainsKey(n))
            flags[n] = v;
        else
            flags.Add(n, v);
    }

    public int var(string v) {
        if (vars.ContainsKey(v))
            return vars[v];
        else return int.MinValue;
    }

    public void setVar(string n, int v) {
        if(!vars.ContainsKey(n))
            vars.Add(n, v);
    }
}
