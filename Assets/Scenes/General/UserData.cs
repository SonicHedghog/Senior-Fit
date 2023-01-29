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

    public UserData(userdata previousData)
    {
        fname = previousData.fname;
        lname = previousData.lname;
        contactno = previousData.contactno;
        LoginTime = previousData.LoginTime;
        version = previousData.version;
    }
}
   