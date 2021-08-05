using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class userdata 
{
    public string name;
    public int contactno;

    public userdata(loginscript newlogin)
    {
        name=newlogin.name;
        contactno=newlogin.contactno;

    }
}
   