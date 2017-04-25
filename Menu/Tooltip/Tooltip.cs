using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tooltip
{

    public static GameObject Create(string _text)
    {
        GameObject mytip = Object.Instantiate(Resources.Load("Tooltip"), new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0)) as GameObject;
        Text myText = mytip.transform.GetChild(0).GetComponent<Text>();
        myText.text = _text;
        mytip.transform.SetParent(GameObject.Find("Canvas").transform);
        mytip.transform.localScale = new Vector3(1, 1, 1);
        return mytip;
    }
}
