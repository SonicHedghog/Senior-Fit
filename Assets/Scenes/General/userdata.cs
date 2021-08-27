using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class userdata 
{
    public string fname;
    public string lname;
    public int contactno;

    public userdata(loginscript newlogin)
    {
        fname=newlogin.fname;
        lname=newlogin.lname;
        contactno=newlogin.contactno;

    }
}
   