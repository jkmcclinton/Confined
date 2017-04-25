using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Evaluator : MonoBehaviour {
    
    private Player player;
    private History history;

    public void Start() {
        player = GameObject.FindObjectOfType<Player>();
        history = GameObject.FindObjectOfType<History>();
    }

    //can be pretty taxing to compute?
    //computes bool algebra from left to right
    public bool evaluate(string origStatement) {
        if (origStatement == null) return true;
        if (String.IsNullOrEmpty(origStatement)) return true;
        string statement = origStatement;

        //		Debug.LogError(statement);

        //split by and/or operators
        List<string> arguments = new List<string>();
        while (statement.Contains(" and ") || statement.Contains(" or ")) {
            int and = statement.IndexOf(" and ");
            int or = statement.IndexOf(" or ");
            int first = statement.IndexOf("(");
            int last = statement.IndexOf(")");
            bool split = false;

            if (first > last || (first == -1 && last != -1) || (first != -1 && last == -1)) {
                Debug.LogError("Mismatch parenthesis; \"" + origStatement + "\"");
                
                return false;
            }

            if (and < or) {
                if (and > -1 && !(and > first && and < last)) {
                    arguments.Add(statement.Substring(0, statement.IndexOf(" and ")-1));
                    statement = statement.Substring(and + " and ".Length);
                    arguments.Add("and");
                    split = true;
                }
                or = statement.IndexOf(" or ");
                if (or > -1 && !(or > first && or < last)) {
                    arguments.Add(statement.Substring(0, statement.IndexOf(" or ")-1));
                    statement = statement.Substring(or + " or ".Length);
                    arguments.Add("or");
                    split = true;
                }
            } else {
                if (or > -1 && !(or > first && or < last)) {
                    arguments.Add(statement.Substring(0, statement.IndexOf(" or ") - 1));
                    statement = statement.Substring(or + " or ".Length);
                    arguments.Add("or");
                    split = true;
                }
                and = statement.IndexOf(" and ");
                if (and > -1 && !(and > first && and < last)) {
                    arguments.Add(statement.Substring(0, statement.IndexOf(" and ") - 1));
                    statement = statement.Substring(and + " and ".Length);
                    arguments.Add("and");
                    split = true;
                }
            }
            if (!split) break;
        }
        arguments.Add(statement);

        if (arguments.Count % 2 != 1) {
            Debug.LogError("Invalid number of arguments; " + origStatement);
        }
        //		Debug.LogError("eval args: "+arguments);
        string s; bool not, result;
        //evaluate individual arguments
        for (int i = 0; i < arguments.Count; i += 2) {
            not = false;
            s = arguments[i].Trim();
            if (s.StartsWith("!")) {
                not = true;
                s = s.Substring(1);
            }

            //evaluate comparisons
            if (s.Contains("[")) { // Contains expressions of format [expression1 comparison expression2]
                if (s.LastIndexOf("]") == -1) {
                    string outMsg = "Mismatch braces; " + origStatement;
                    Debug.LogError(outMsg);
                    return false;
                }
                result = evaluateComparison(s.Substring(s.IndexOf("[") + 1,
                        s.LastIndexOf("]") - s.IndexOf("[")));
                //evaluate parentheticals by calling this method
            } else if (s.Contains("(")) { // Contains format (bool1 operator bool2)
                result = evaluate(s.Substring(s.IndexOf("(") + 1, s.LastIndexOf(")")- s.IndexOf("(")));
                //evaluate bool variables
            } else {
                string val = null;

                val = (history.flag(s)).ToString();
                if (val != null)
                    result = Boolean.Parse(val);
                else
                    result = false;
            }

            if (not) result = !result;
            arguments[i] = (result).ToString();
        }
        //		Debug.LogError(arguments);

        //combine by operators from left to right
        //[true, and, false] >> [false]
        while (arguments.Count >= 3) {
            if (arguments[1].Equals("or")) {
                arguments[0] = (Boolean.Parse(arguments[0]) ||
                        Boolean.Parse(arguments[2])).ToString();
            } else if (arguments[1].Equals("and")) {
                arguments[0] = (Boolean.Parse(arguments[0]) &&
                        Boolean.Parse(arguments[2])).ToString();
            } else {
                Debug.LogError("\"" + arguments[0] + "\" is not a known logical operator");
            }

            arguments.RemoveAt(1);
            arguments.RemoveAt(1);
        }
        //		Debug.LogError(arguments);
        return Boolean.Parse(arguments[0]);
    }

    /// <summary>
    /// determines the bool result of a comparison between expressions and variables
    /// </summary>
    private bool evaluateComparison(string statement) {
        string obj, property = null;
        bool result = false;

        if (statement.Contains(">") || statement.Contains("<") || statement.Contains("=")) {
            string value = "", val = "";

            //separate value and condition from tmp
            obj = "";
            string condition = "";
            string tmp = statement.Replace(" ", ""), index;
            int first = -1;
            for (int i = 0; i < tmp.Length - 1; i++) {
                index = tmp.Substring(i, 1);
                if ((index.Equals(">") || index.Equals("<") || index.Equals("=") ||
                        (index.Equals("!") && tmp.Contains("!="))) && condition.Length < 2) {
                    if (first == -1) {
                        condition += index;
                        first = i;
                    } else if (i - first == 1)
                        condition += index;
                }
            }

            if (tmp.IndexOf(condition) < 1) {
                Debug.LogError("No object found to compare with in statement: " + statement);
                    return false;
            }

            obj = tmp.Substring(0, tmp.IndexOf(condition));
            val = tmp.Substring(tmp.IndexOf(condition) + condition.Length);

            if (obj.Contains("+") || obj.Contains("-") || obj.Contains("*") || obj.Contains("/"))
                property = evaluateExpression(obj);
            else
                property = determineValue(obj);

            if (val.Contains("+") || val.Contains("-") || val.Contains("*") || val.Contains("/"))
                value = evaluateExpression(val);
            else
                value = determineValue(val);

            //			Debug.LogError(statement);
            //			Debug.LogError("p: " + property + "\tc: " + condition + "\tv: " + value);

            //actual comparator
            try {
                switch (condition) {
                    case "=":
                        if (Vars.isNumeric(property) && Vars.isNumeric(value))
                            result = (double.Parse(property) == double.Parse(value));
                        else
                            result = property.Equals(value);
                        break;
                    case "!=":
                        if (Vars.isNumeric(property) && Vars.isNumeric(value))
                            result = (double.Parse(property) != double.Parse(value));
                        else
                            result = !property.Equals(value);
                        break;
                    case ">":
                        if (Vars.isNumeric(property) && Vars.isNumeric(value))
                            result = (double.Parse(property) > double.Parse(value));
                        break;
                    case ">=":
                    case "=>":
                        if (Vars.isNumeric(property) && Vars.isNumeric(value))
                            result = (double.Parse(property) >= double.Parse(value));
                        break;
                    case "<":
                        if (Vars.isNumeric(property) && Vars.isNumeric(value))
                            result = (double.Parse(property) < double.Parse(value));
                        break;
                    case "<=":
                    case "=<":
                        if (Vars.isNumeric(property) && Vars.isNumeric(value))
                            result = (double.Parse(property) <= double.Parse(value));
                        break;
                    default:
                        string outMsg = "\"" + condition + "\" is not a vaild operator";
                        Debug.LogError(outMsg);
                        break;
                }

                //				Debug.LogError("result: "+result);
                return result;
            } catch (Exception) {
                string outMsg = "Could not compare \"" + property + "\" with \"" + value + "\" by condition \"" + condition + "\"";
                Debug.LogError(outMsg);
                return false;
            }
        } else {
            string outMsg = "No mathematical expression found in statement \"" + statement + "\"";
            Debug.LogError(outMsg);
            return false;
        }
    }

    /// <summary>
    /// returns the solution to a set of mathematical operators; does not handle parsing of negative numbers!
    /// </summary>
    public string evaluateExpression(string obj) {
        List<string> arguments;
        string result, res, val;
        string tmp = obj.Remove(' ');
        tmp = tmp.Replace("-", "&");

        for (int i = 0; i < tmp.Length - 1; i++) {
            if (tmp.Substring(i, 1).Equals("(")) {
                if (tmp.LastIndexOf(")") == -1) {
                    string outMsg = "Error evaluating: \"" + tmp + "\"\nMissing a \")\"";
                    Debug.LogError(outMsg);
                    return null;
                }
                string e = evaluateExpression(tmp.Substring(i + 1, tmp.LastIndexOf(")")));
                tmp = tmp.Substring(0, i) + e
                        + tmp.Substring(tmp.LastIndexOf(")") + 1);
                i = tmp.LastIndexOf(")");
            }
        }

        //TODO sort expressions
        //not programmed, sorry

        //evaluate
        //separates all arguments and contstants from operators
        arguments = new List<string>(Regex.Split(tmp, @"(?<=[&+*/])|(?=[&+*/])"));
        if (arguments.Count < 3 || arguments.Contains("")) return obj;
        //		Debug.LogError(arguments);

        result = arguments[0];
        if (!Vars.isNumeric(result)) {
            res = determineValue(result);
            if (res != null)
                result = res.Clone().ToString();
        }

        //continuously evaluate operations until there aren't enough
        //left to perform a single operation
        //[1, +, 2] >> [3]
        while (arguments.Count >= 3) {
            val = arguments[2];
            switch (arguments[1]) {
                case "+":
                    if (Vars.isNumeric(result) && Vars.isNumeric(val)) {
                        result = (float.Parse(result) + float.Parse(val)).ToString();
                    } else {
                        res = determineValue(result);
                        if (res != null)
                            result = res.Clone().ToString();

                        res = determineValue(val);
                        if (res != null)
                            val = res.Clone().ToString();

                        if (Vars.isNumeric(result) && Vars.isNumeric(val))
                            result = (float.Parse(result) + float.Parse(val)).ToString();
                        else result += val;
                    }
                    break;
                case "&":
                    if (Vars.isNumeric(result) && Vars.isNumeric(val)) {
                        result = (float.Parse(result) - float.Parse(val)).ToString();
                    } else {
                        res = determineValue(result);
                        if (res != null)
                            result = res.Clone().ToString();

                        res = determineValue(val);
                        if (res != null)
                            val = res.Clone().ToString();

                        if (Vars.isNumeric(result) && Vars.isNumeric(val))
                            result = (float.Parse(result) - float.Parse(val)).ToString();
                        else Debug.LogError("Conversion error: res: \"" + result + "\" op: - val: \"" + val);
                    }
                    break;
                case "*":
                    if (Vars.isNumeric(result) && Vars.isNumeric(val)) {
                        result = (float.Parse(result) * float.Parse(val)).ToString();
                    } else {
                        res = determineValue(result);
                        if (res != null)
                            result = res.Clone().ToString();

                        res = determineValue(val);
                        if (res != null)
                            val = res.Clone().ToString();

                        if (Vars.isNumeric(result) && Vars.isNumeric(val))
                            result = (float.Parse(result) * float.Parse(val)).ToString();
                        else Debug.LogError("Conversion error: res: \"" + result + "\" op: - val: \"" + val);
                    }
                    break;
                case "/":
                    if (Vars.isNumeric(result) && Vars.isNumeric(val)) {
                        result = (float.Parse(result) / float.Parse(val)).ToString();
                    } else {
                        res = determineValue(result);
                        if (res != null)
                            result = res.Clone().ToString();

                        res = determineValue(val);
                        if (res != null)
                            val = res.Clone().ToString();

                        if (Vars.isNumeric(result) && Vars.isNumeric(val))
                            result = (float.Parse(result) / float.Parse(val)).ToString();
                        else Debug.LogError("Conversion error: res: \"" + result + "\" op: - val: \"" + val);
                    }
                    break;
            }

            arguments.RemoveAt(1);
            arguments.RemoveAt(1);
            //			Debug.LogError("removing: "+arguments);
        }

        return result;
    }

    //determine whether argument is and object property, variable, flag, or event
    //by default, if no object can be found it is automatically assumed to be an event
    //should possibly change in the future;
    private string determineValue(string obj) {
        string not = "", property = null;
        string orig = obj;
        try {
            //ensure that invalid characters do not change the outcome
            obj = (obj.Replace("[", "")).Replace("]", "");

            if (Vars.isNumeric(obj))
                property = obj;
            //random between [0,10]
            else if (obj.ToLower().Equals("random"))
                return (UnityEngine.Random.Range(0, 1) * 10).ToString();
            else {
                // find variable
                int i = history.var(obj);
                if (i != int.MinValue)
                    property = i + "";
                else {
                    //Debug.Log("flag: " + obj);
                    if (obj.StartsWith("!")) {
                        not = "!";
                        obj = obj.Substring(1);
                    }

                    //find flag or event
                    property = not + (history.flag(obj)).ToString();
                }
            }

            return property;
        } catch (Exception) {
            //e.printStackTrace();
            Debug.LogError("FATAL error trying to determine value for \"" + orig + "\"");
            return null;
        }
    }
}
