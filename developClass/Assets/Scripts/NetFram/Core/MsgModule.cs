﻿using common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ko.NetFram
{
    public class MsgModule : imodule
    {

        public static MsgModule getInstance()
        {
            return Singleton<MsgModule>.getInstance();
        }

        private msg_req_ret _handler;
        private sync_req_ret _synchamdler;
        private sync_commond _synccommond;
        private sync_string_message _listenmsg;

        public void registerSyncCommond(sync_commond synccommond)
        {
            _synccommond = synccommond;
        }

        public void registerMsgHandler(msg_req_ret msghandler)
        {
            _handler = msghandler;
        }

        public void registerSyncMsgHandler(sync_req_ret syncmsghandler)
        {
            _synchamdler = syncmsghandler;
        }

        public void registerListenMsgHandler(sync_string_message listenmsghandler)
        {
            _listenmsg = listenmsghandler;
        }

        // 向服务器发送消息
        public void req_msg(Hashtable msg, string contactmodule = null, string contactfunc = null)
        {
            // sMsgConnect ret_msg 默认
            if (contactmodule == null)
            {
                contactmodule = "sMsgConnect";
            }

            if (contactfunc == null)
            {
                contactfunc = "ret_msg";
            }

            Debug.Log(UserInfor.getInstance().RoomName);

            // 调用 hub 层
            MainThreadClient._client.call_hub("lobby", contactmodule, contactfunc, UserInfor.getInstance().RoomName, msg);
        }

        // 接收服务器返回
        public void ret_msg(Hashtable msg)
        {
            if (_handler != null)
            {
                _handler.ret_msg(msg);
            }

        }

        // 同步vextor3变量 可以是position rotation
        public void req_sync_vector3(float x, float y, float z, Config.SyncTransform prs, string objectname, string uuid, string contactmodule = null, string contactfunc = null)
        {
            // sMsgConnect ret_msg 默认
            if (contactmodule == null)
            {
                contactmodule = UserInfor.getInstance().Connector;
            }

            if (contactmodule == null)
            {
                return;
            }

            if (contactfunc == null)
            {
                contactfunc = "ret_sync_msg";
            }

            if (uuid == null)
            {
                uuid = UserInfor.getInstance().UserUuid;
            }

            // 调用 hub 层
  //          MainThreadClient._client.call_hub("lobby", contactmodule, contactfunc, x, y, z, objectname, uuid);
        }

        public void req_sync_vector4(float x, float y, float z, float w, string objectname, string uuid, string contactmodule = null, string contactfunc = null)
        {
            if (contactmodule == null)
            {
                contactmodule = UserInfor.getInstance().Connector;
            }

            if (contactmodule == null)
            {
                return;
            }

            if (contactfunc == null)
            {
                contactfunc = "ret_sync_vector4";
            }

            if (uuid == null)
            {
                uuid = UserInfor.getInstance().UserUuid;
            }

            // 调用 hub 层
            MainThreadClient._client.call_hub("lobby", contactmodule, contactfunc, x, y, z, w, objectname, uuid);
        }

        public void req_sync_vector6(float x, float y, float z, float sx, float sy, float sz, float sw, string objectname, string uuid, string contactmodule = null, string contactfunc = null)
        {
            if (contactmodule == null)
            {
                contactmodule = UserInfor.getInstance().Connector;
            }

            if (contactmodule == null)
            {
                return;
            }

            if (contactfunc == null)
            {
                contactfunc = "ret_sync_vector6";
            }

            if (uuid == null)
            {
                uuid = UserInfor.getInstance().UserUuid;
            }

            // 调用 hub 层
            MainThreadClient._client.call_hub("lobby", contactmodule, contactfunc, x, y, z, sx, sy, sz, sw, objectname, uuid);
        }

        // 指令同步
        public void req_sync_commond(string typ, string commond, string token, string other, string uuid, string contactmodule = null, string contactfunc = null)
        {
            if (contactmodule == null)
            {
                contactmodule = UserInfor.getInstance().Connector;
            }

            if (contactmodule == null)
            {
                return;
            }

            if (contactfunc == null)
            {
                contactfunc = "ret_sync_commond";
            }

            if (uuid == null)
            {
                uuid = UserInfor.getInstance().UserUuid;
            }

            // 调用 hub 层
            MainThreadClient._client.call_hub("lobby", contactmodule, contactfunc, UserInfor.getInstance().RoomName, typ, commond, token, other, uuid);
        }

        //指令同步
        public void ret_sync_commond(string typ, string commond, string token, string other)
        {
            Debug.Log("收到同步指令" + typ + " commond " + commond);
            if (_synccommond != null)
            {
                _synccommond.ret_sync_commond(typ, commond, token, other);
            }
        }

        // 接收服务器string消息
        public void ListenerServerMsg(string msg)
        {
            Debug.Log("服务器通知消息" + msg);

            if(_listenmsg != null)
            {
                _listenmsg.listen_string_msg(msg);
            }
        }

        // 同步服务器场景数据
        public void SyncClient(Hashtable msgObject, Hashtable msgPlayer)
        {
            Debug.Log("客户端收到服务器同步物体玩家信息");

            ReciveObjectOnce.getInstance().ReciveSyncObject(msgObject, msgPlayer);
        }

        // 同步服务器场景信息和指令等
        public void SyncClientAndCommond(Hashtable msgObject, Hashtable msgPlayer, Hashtable msgCommond)
        {
            Debug.Log("收到场景同步指令 包括 数据 " + msgObject.Count + " 和 玩家 " + msgPlayer.Count + "和 指令 " + msgCommond.Count);
        }

        // 一次性同步此客户端所有改变数据
        public void req_all_change_once(Hashtable msg, string token = null, string contactmodule = null, string contactfunc = null)
        {
            if(token == null)
            {
                token = UserInfor.getInstance().UserToken;
            }

            if(token == null)
            {
                return;
            }

            if(contactmodule == null)
            {
                contactmodule = UserInfor.getInstance().RoomConnecter;
            }

            if (contactmodule == null)
            {
                return;
            }

            if (contactfunc == null)
            {
                contactfunc = "ChangeObjectAllOnce";
            }

            MainThreadClient._client.call_hub("lobby", contactmodule, contactfunc, UserInfor.getInstance().RoomName, token, msg);

        }

        // 操作物体锁请求
        public void req_operation_permission(string token, string objectname, string uuid, string contactmodule = null, string contactfunc = null)
        {
            if (token == null)
            {
                token = UserInfor.getInstance().UserToken;
            }

            if (token == null)
            {
                return;
            }

            if (contactmodule == null)
            {
                contactmodule = UserInfor.getInstance().RoomConnecter;
            }

            if (contactmodule == null)
            {
                return;
            }

            if (contactfunc == null)
            {
                contactfunc = "Req_Object_Operate_permissions";
            }

            if (uuid == null)
            {
                uuid = UserInfor.getInstance().UserUuid;
            }

            if (uuid == null)
            {
                return;
            }

            MainThreadClient._client.call_hub("lobby", contactmodule, contactfunc, UserInfor.getInstance().RoomName, token, objectname, uuid);
        }

        public void req_operation_release(string token, string objectname, string uuid, string contactmodule = null, string contactfunc = null)
        {
            if (token == null)
            {
                token = UserInfor.getInstance().UserToken;
            }

            if (token == null)
            {
                return;
            }

            if (contactmodule == null)
            {
                contactmodule = UserInfor.getInstance().RoomConnecter;
            }

            if (contactmodule == null)
            {
                return;
            }

            if (contactfunc == null)
            {
                contactfunc = "Req_Object_Release_permissions";
            }

            if (uuid == null)
            {
                uuid = UserInfor.getInstance().UserUuid;
            }

            if (uuid == null)
            {
                return;
            }

            MainThreadClient._client.call_hub("lobby", contactmodule, contactfunc, UserInfor.getInstance().RoomName, token, objectname, uuid);
        }

        public void ret_operation_permissions(string token, string objectname, string typ, string result)
        {
            Debug.Log(token + "请求操作返回" + objectname);

            if(objectname == null || result == null || token == null)
            {
                return;
            }

            SceneObject go = CollectionObject.getInstance().getSceneObjectByKey(objectname);
            if (go == null)
            {
                Debug.Log("该物体不存在 " + objectname);
            }

            go.listen_operate_result(token, typ, result);
        }

        // 模式相关 ----------------------------   模式相关
        public void req_switch_model(string token, string tomodel, string uuid, string contactmodule = null, string contactfunc = null)
        {
            if(token == null)
            {
                token = UserInfor.getInstance().UserToken;
            }

            if (token == null)
            {
                return;
            }

            if (contactmodule == null)
            {
                contactmodule = UserInfor.getInstance().RoomConnecter;
            }

            if (contactmodule == null)
            {
                return;
            }

            if (contactfunc == null)
            {
                contactfunc = "Switch_Model";
            }

            if (uuid == null)
            {
                uuid = UserInfor.getInstance().UserUuid;
            }

            // 调用 hub 层
            MainThreadClient._client.call_hub("lobby", contactmodule, contactfunc, UserInfor.getInstance().RoomName, token, tomodel, uuid);
        }

        // 改变某一学生的模式状态 
        public void req_change_one_model(string token, string tomodel, string uuid, string onetoken, string contactmodule = null, string contactfunc = null)
        {
            if (token == null)
            {
                token = UserInfor.getInstance().UserToken;
            }

            if (token == null)
            {
                return;
            }

            if (contactmodule == null)
            {
                contactmodule = UserInfor.getInstance().RoomConnecter;
            }

            if (contactmodule == null)
            {
                return;
            }

            if (contactfunc == null)
            {
                contactfunc = "Change_One_Model";
            }

            if (uuid == null)
            {
                uuid = UserInfor.getInstance().UserUuid;
            }

            if(onetoken == null)
            {
                return;
            }

            // 调用 hub 层
            MainThreadClient._client.call_hub("lobby", contactmodule, contactfunc, UserInfor.getInstance().RoomName, token, tomodel, uuid, onetoken);
        }

        // 改变某些学生的模式状态 
        public void req_change_some_model(string token, string tomodel, string uuid, ArrayList sometoken, string contactmodule = null, string contactfunc = null)
        {
            if (token == null)
            {
                token = UserInfor.getInstance().UserToken;
            }

            if (token == null)
            {
                return;
            }

            if (contactmodule == null)
            {
                contactmodule = UserInfor.getInstance().RoomConnecter;
            }

            if (contactmodule == null)
            {
                return;
            }

            if (contactfunc == null)
            {
                contactfunc = "Change_Some_Model";
            }

            if (uuid == null)
            {
                uuid = UserInfor.getInstance().UserUuid;
            }

            if(sometoken == null || sometoken.Count <= 0)
            {
                return;
            }

            // 调用 hub 层
            MainThreadClient._client.call_hub("lobby", contactmodule, contactfunc, UserInfor.getInstance().RoomName, token, tomodel, uuid, sometoken);
        }

        // 接收返回

        public void ret_change_one_model(string token, string tomodel)
        {
            Debug.Log("ret_change_one_model " + token + " model " + tomodel);
            UserInfor.getInstance().ChangePlayerModel(tomodel);
        }
    }
}
