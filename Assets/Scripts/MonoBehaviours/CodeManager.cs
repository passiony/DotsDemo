using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeManager : MonoBehaviour
{
    private ConnectViaCode connectViaCode;
    public Dictionary<string, Faction> Users;

    void Start()
    {
        connectViaCode = gameObject.GetComponent<ConnectViaCode>();
        Users = new Dictionary<string, Faction>();
        connectViaCode.OnDanmaku += OnDanmaku;
    }

    //join,red
    //gen,123,1
    private void OnDanmaku(string username, string msg)
    {
        //加入阵营
        var arr = msg.Split(",");
        if (arr.Length == 2)
        {
            if (arr[0].ToLower() == "join" && Enum.TryParse(arr[1],true, out Faction faction))
            {
                Users[username] = faction;
            }
        }
        //生成士兵
        else if (arr.Length == 3)
        {
            if (arr[0].ToLower() == "gen" && int.TryParse(arr[1], out int code) && int.TryParse(arr[2], out int count))
            {
                if (Users.TryGetValue(username, out var faction))
                {
                    if (code >= 1 && code <= 3)
                    {
                        var solder = (SolderType)(code - 1);
                        GameManager.Instance.Gen(faction, solder, count);
                    }

                }
            }
        }
    }
}