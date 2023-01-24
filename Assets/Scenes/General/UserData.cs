using System;

[System.Serializable]
public class UserData 
{
    public string fname;
    public string lname;
    public long contactno;
    public string version;
    public DateTime LoginTime;

    public UserData(LoginSetUp newlogin)
    {
        fname = newlogin.fname;
        lname = newlogin.lname;
        contactno = newlogin.contactno;
        LoginTime = newlogin.LoginTime;
        version = newlogin.version;
    }
}
   