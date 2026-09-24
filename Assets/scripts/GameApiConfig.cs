using System;
using UnityEngine;

public class GameApiConfig
{
    public static string ClientUUID
    {
        get
        {
            string _UUID = PlayerPrefs.GetString("ClientUUID", "");
            if (string.IsNullOrEmpty(_UUID))
            {
                _UUID = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("ClientUUID", _UUID);
                PlayerPrefs.Save();
            }
            return _UUID;
        }
    }


}
