using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Interactable))]
public class Interactor : Editor {

    private int index = -1;
    GameObject go;
    List<string> scrOpt;
    
    void OnEnable() {
        scrOpt = new List<string>(Vars.SCRIPT_LIST.Keys);
        Interactable iB = ((Interactable)target);
        index = scrOpt.IndexOf(iB.scriptFile);
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        Interactable iB = (Interactable)target;

        int i = EditorGUILayout.Popup("Script Source", index < 0 ? 0 : index, scrOpt.ToArray());
        if (i != index) {
            iB.scriptFile = scrOpt[i];
            index = i;
        }
    }
}
