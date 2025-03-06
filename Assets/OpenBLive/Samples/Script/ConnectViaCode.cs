using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using Newtonsoft.Json;
using OpenBLive.Runtime;
using OpenBLive.Runtime.Data;
using OpenBLive.Runtime.Utilities;
using UnityEngine;
using Logger = OpenBLive.Runtime.Utilities.Logger;

public class ConnectViaCode : MonoBehaviour
{
    public static ConnectViaCode Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ConnectViaCode>();
            }

            return instance;
        }
    }

    private static ConnectViaCode instance;

    // Start is called before the first frame update
    private WebSocketBLiveClient m_WebSocketBLiveClient;
    private InteractivePlayHeartBeat m_PlayHeartBeat;
    private string gameId;
    public string accessKeySecret;
    public string accessKeyId;
    public string appId;

    public Action ConnectSuccess;
    public Action ConnectFailure;
    public Action<string, string> OnDanmaku;

    public async void LinkStart(string code)
    {
        //测试的密钥
        SignUtility.accessKeySecret = accessKeySecret;
        //测试的ID
        SignUtility.accessKeyId = accessKeyId;
        var ret = await BApi.StartInteractivePlay(code, appId);
        //打印到控制台日志
        var gameIdResObj = JsonConvert.DeserializeObject<AppStartInfo>(ret);
        if (gameIdResObj.Code != 0)
        {
            Debug.LogError(gameIdResObj.Message);
            ConnectFailure?.Invoke();
            return;
        }

        //长链接（用户持续接收服务器推送消息）
        m_WebSocketBLiveClient = new WebSocketBLiveClient(gameIdResObj.GetWssLink(), gameIdResObj.GetAuthBody());
        m_WebSocketBLiveClient.OnDanmaku += WebSocketBLiveClientOnDanmaku; //弹幕事件
        m_WebSocketBLiveClient.OnGift += WebSocketBLiveClientOnGift; //礼物事件
        m_WebSocketBLiveClient.OnGuardBuy += WebSocketBLiveClientOnGuardBuy; //大航海事件
        m_WebSocketBLiveClient.OnSuperChat += WebSocketBLiveClientOnSuperChat; //SC事件
        m_WebSocketBLiveClient.OnLike += M_WebSocketBLiveClient_OnLike; //点赞事件(点赞需要直播间开播才会触发推送)
        m_WebSocketBLiveClient.OnEnter += M_WebSocketBLiveClient_OnEnter; //观众进入房间事件
        m_WebSocketBLiveClient.OnLiveStart += M_WebSocketBLiveClient_OnLiveStart; //直播间开始直播事件
        m_WebSocketBLiveClient.OnLiveEnd += M_WebSocketBLiveClient_OnLiveEnd; //直播间停止直播事件

        try
        {
            m_WebSocketBLiveClient.Connect(TimeSpan.FromSeconds(1), 1000000);
            ConnectSuccess?.Invoke();
            Debug.Log("连接成功");
        }
        catch (Exception ex)
        {
            ConnectFailure?.Invoke();
            Debug.Log("连接失败");
            throw;
        }

        gameId = gameIdResObj.GetGameId();

        //心跳API（用于保持在线）
        m_PlayHeartBeat = new InteractivePlayHeartBeat(gameId);
        m_PlayHeartBeat.HeartBeatError += M_PlayHeartBeat_HeartBeatError;
        m_PlayHeartBeat.HeartBeatSucceed += M_PlayHeartBeat_HeartBeatSucceed;
        m_PlayHeartBeat.Start();
    }


    public async Task LinkEnd()
    {
        m_WebSocketBLiveClient.Dispose();
        m_PlayHeartBeat.Dispose();
        await BApi.EndInteractivePlay(appId, gameId);
        Debug.Log("游戏关闭");
    }

    private static void M_WebSocketBLiveClient_OnLiveEnd(LiveEnd liveEnd)
    {
        StringBuilder sb =
            new StringBuilder($"直播间[{liveEnd.room_id}]直播结束，分区ID：【{liveEnd.area_id}】,标题为【{liveEnd.title}】");
        Logger.Log(sb.ToString());
    }

    private static void M_WebSocketBLiveClient_OnLiveStart(LiveStart liveStart)
    {
        StringBuilder sb =
            new StringBuilder($"直播间[{liveStart.room_id}]开始直播，分区ID：【{liveStart.area_id}】,标题为【{liveStart.title}】");
        Logger.Log(sb.ToString());
    }

    private static void M_WebSocketBLiveClient_OnEnter(Enter enter)
    {
        StringBuilder sb = new StringBuilder($"用户[{enter.uname}]进入房间");
        Logger.Log(sb.ToString());
    }

    private static void M_WebSocketBLiveClient_OnLike(Like like)
    {
        StringBuilder sb = new StringBuilder($"用户[{like.uname}]点赞了{like.unamelike_count}次");
        Logger.Log(sb.ToString());
    }

    private static void WebSocketBLiveClientOnSuperChat(SuperChat superChat)
    {
        StringBuilder sb = new StringBuilder($"用户[{superChat.userName}]发送了{superChat.rmb}元的醒目留言内容：{superChat.message}");
        Logger.Log(sb.ToString());
    }

    private static void WebSocketBLiveClientOnGuardBuy(Guard guard)
    {
        StringBuilder sb =
            new StringBuilder(
                $"用户[{guard.userInfo.userName}]充值了{(guard.guardUnit == "月" ? (guard.guardNum + "个月") : guard.guardUnit.TrimStart('*'))}[{(guard.guardLevel == 1 ? "总督" : guard.guardLevel == 2 ? "提督" : "舰长")}]大航海");
        Logger.Log(sb.ToString());
    }

    private static void WebSocketBLiveClientOnGift(SendGift sendGift)
    {
        StringBuilder sb = new StringBuilder($"用户[{sendGift.userName}]赠送了{sendGift.giftNum}个[{sendGift.giftName}]");
        Logger.Log(sb.ToString());
    }

    private static void WebSocketBLiveClientOnDanmaku(Dm dm)
    {
        StringBuilder sb = new StringBuilder($"用户[{dm.userName}]发送弹幕:{dm.msg}");
        Logger.Log(sb.ToString());

        Instance.OnDanmaku?.Invoke(dm.userName, dm.msg);
    }


    private static void M_PlayHeartBeat_HeartBeatSucceed()
    {
        Debug.Log("心跳成功");
    }

    private static void M_PlayHeartBeat_HeartBeatError(string json)
    {
        Debug.Log("心跳失败" + json);
    }


    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (m_WebSocketBLiveClient is { ws: { State: WebSocketState.Open } })
        {
            m_WebSocketBLiveClient.ws.DispatchMessageQueue();
        }
#endif
    }

    private void OnDestroy()
    {
        if (m_WebSocketBLiveClient == null)
            return;

        m_PlayHeartBeat.Dispose();
        m_WebSocketBLiveClient.Dispose();
    }
}