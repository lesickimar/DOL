using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    /*
    public static GameObject Create(string _text)
    {
        GameObject mytip = Object.Instantiate(Resources.Load("Tooltip"), new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0)) as GameObject;
        Text myText = mytip.transform.GetChild(0).GetComponent<Text>();
        myText.text = _text;
        mytip.transform.SetParent(GameObject.Find("Canvas").transform);
        mytip.transform.localScale = new Vector3(1, 1, 1);
        return mytip;
    }
    */

    public bool followPointer;
    public int duration;
    public int timer;
    public string content;

    public static GameObject Show(string _content, int _duration, Vector3 position, bool _followPointer)
    {
        GameObject tipObject = Instantiate(Resources.Load("Tooltip")) as GameObject;
        Tooltip myTip = tipObject.GetComponent<Tooltip>();
        myTip.content = _content;
        myTip.duration = _duration;
        tipObject.transform.position = position;
        myTip.followPointer = _followPointer;

        return tipObject;
    }

    private void Update()
    {
        if (followPointer)
            FollowPointer();

        if (timer++ >= duration)
        {
            Destroy(this);
        }
    }

    private void FollowPointer()
    {
        Vector3 v3 = Input.mousePosition;
        v3.z = 10f;
        v3 = Camera.main.ScreenToWorldPoint(v3);
        transform.position = Camera.main.ScreenToWorldPoint(v3);
    }
}
