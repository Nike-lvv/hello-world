﻿using common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace veClassRoom.Room
{
    class WisdomLogin : imodule
    {
        public static string selfmodelname = "WisdomLogin";

        public static Dictionary<Int64, UserInfor> allplayerlogin = new Dictionary<Int64, UserInfor>();

        public void player_login(string password, string name, string modelname, string callbackname)
        {
            var client_uuid = hub.hub.gates.current_client_uuid;

            if (modelname == null)
            {
                modelname = "cMsgConnect";
            }

            if (callbackname == null)
            {
                callbackname = "ret_msg";
            }

            BackDataService.getInstance().CheckUser(name, password, this.Login_Succeed, this.Login_Failure, client_uuid);

        }

        public void Login_Succeed(BackDataType.PlayerLoginRetData v, string tag)
        {
            if(tag == null)
            {
                return;
            }

            try
            {
                UserInfor user = new UserInfor();
                user.InitLoginRetData(v);
                user.uuid = tag;

                Int64 id = Convert.ToInt64(user.id);
                if (allplayerlogin.ContainsKey(id))
                {
                    allplayerlogin[id] = user;
                }
                else
                {
                    allplayerlogin.Add(id, user);
                }

                player_base_infor(v.data.access_token, tag);
            }
            catch
            {

            }
        }

        public void Login_Failure(BackDataType.MessageRetHead errormsg, string tag)
        {
            if (tag == null)
            {
                return;
            }

            try
            {
                Hashtable h = new Hashtable();
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Result, "failed");
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_ErrorMsg, errormsg.message);

                hub.hub.gates.call_client(tag, "cMsgConnect", "ret_msg", h);
            }
            catch
            {

            }
        }

        // 获取用户基本信息
        public void player_base_infor(string token, string uuid)
        {
            BackDataService.getInstance().GetPlayerBaseInfor(token, player_base_infor_Succeed, player_base_infor_Failure, uuid);
        }

        public void player_base_infor_Succeed(BackDataType.PlayerBaseInforRetData v, string tag)
        {
            if (tag == null)
            {
                return;
            }

            try
            {
                Int64 id = Convert.ToInt64(v.data.user_id);
                if (!allplayerlogin.ContainsKey(id))
                {
                    return;
                }

                UserInfor user = null;
                allplayerlogin[id].InitBaseInforRetData(v);
                user = allplayerlogin[id];

                Hashtable h = new Hashtable();
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Result, "success");
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Token, user.access_token);
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Name, user.name);
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Uuid, user.uuid);
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Id, user.id);
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Duty, user.identity);

                hub.hub.gates.call_client(tag, "cMsgConnect", "ret_msg", h);
            }
            catch
            {}
        }

        public void player_base_infor_Failure(BackDataType.MessageRetHead errormsg, string tag)
        {
            if (tag == null)
            {
                return;
            }

            try
            {
                Hashtable h = new Hashtable();
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Result, "failed");
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_ErrorMsg, errormsg.message);

                hub.hub.gates.call_client(tag, "cMsgConnect", "ret_msg", h);
            }
            catch
            {

            }
        }

        public void player_exit(string token, string uuid, Int64 userid, Int64 roomid, string modelname = null, string callbackname = null)
        {
            if (modelname == null)
            {
                modelname = "cMsgConnect";
            }

            if (callbackname == null)
            {
                callbackname = "ret_msg";
            }

            Console.WriteLine("用户退出 : " + " token:" + token + " uuid:" + uuid);

            // 退出场景
            RealRoom rr = RoomManager.getInstance().FindRoomById(roomid);

            if (rr != null)
            {
                rr.PlayerLeaveScene(token, uuid);
            }

            //向服务器发送用户的个人数据
            // TODO

            // 登陆列表移除
            if(allplayerlogin.ContainsKey(userid))
            {
                allplayerlogin.Remove(userid);
            }
        }

        // 玩家进入房间
        public void player_enter_room(string token, string name, string uuid, string roomname, string modelname, string callbackname)
        {
            Console.WriteLine("用户请求进入房间 : " + "name:" + name + " token:" + token + " uuid:" + uuid + " roomname:" + roomname);

            RealRoom rr = RoomManager.getInstance().CreateRoomByName(roomname);

            Hashtable h = new Hashtable();

            if (rr == null)
            {
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Result, "failed");
            }
            else
            {
   //             rr.PlayerEnterScene(token, name, uuid);
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Result, "success");
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Scene_name, rr.scenename);
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Connector, NetMessage.selfmodelname);
            }

            if (modelname == null)
            {
                modelname = "cMsgConnect";
            }

            if (callbackname == null)
            {
                callbackname = "ret_msg";
            }

            hub.hub.gates.call_client(uuid, modelname, callbackname, h);

        }

        public void EnterLobby(string token,string uuid, Int64 duty)
        {
            switch(duty)
            {
                case 1:
                    // 获取学生课程
                    BackDataService.getInstance().GetStudentCourseList(token, Enter_Lobby_Succeed, Enter_Lobby_Failure, uuid);
                    break;
                case 2:
                    // 获取老师课程
                    BackDataService.getInstance().GetTeacherCourseList(token, "vr", Enter_Lobby_Succeed, Enter_Lobby_Failure, uuid);
                    break;
                default:
                    break;
            }
        }

        public void Enter_Lobby_Succeed(BackDataType.CourseListRetData v, string tag)
        {
            if (tag == null)
            {
                return;
            }

            try
            {
                Hashtable h = BackDataType.CourseListRetData_Serialize(v);
                if(h!=null)
                {
                    h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Result, "success");
                    hub.hub.gates.call_client(tag, "cMsgConnect", "ret_msg", h);
                }
                                
            }
            catch
            {

            }
        }

        public void Enter_Lobby_Failure(BackDataType.MessageRetHead errormsg, string tag)
        {
            if (tag == null)
            {
                return;
            }

            try
            {
                Hashtable h = new Hashtable();
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Result, "failed");
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_ErrorMsg, errormsg.message);

                hub.hub.gates.call_client(tag, "cMsgConnect", "ret_msg", h);
            }
            catch
            {

            }
        }

        // 进入课程 创建房间 老师信息 学生信息等
        public void EnterCourse(Int64 userid, string uuid, Int64 courseid)
        {
            RealRoom rr = RoomManager.getInstance().CreateRoomById(courseid);

            Hashtable h = new Hashtable();

            if (rr == null)
            {
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Result, "failed");
            }
            else
            {
                if(!allplayerlogin.ContainsKey(userid))
                {
                    return;
                }

                rr.PlayerEnterScene(allplayerlogin[userid]);
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Result, "success");
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Class_id, courseid.ToString());
                h.Add(ConstantsDefine.HashTableKeyEnum.Net_Ret_Connector, NetMessage.selfmodelname);
            }

            hub.hub.gates.call_client(uuid, "cMsgConnect", "ret_msg", h);
        }

        public void Enter_Course_Succeed(BackDataType.CourseListRetData v, string tag)
        {
            if (tag == null)
            {
                return;
            }

            try
            {
                Hashtable h = BackDataType.CourseListRetData_Serialize(v);
                if (h != null)
                {
                    hub.hub.gates.call_client(tag, "cMsgConnect", "ret_msg", h);
                }

            }
            catch
            {

            }
        }

        public void Enter_Course_Failure(BackDataType.MessageRetHead errormsg, string tag)
        {
            if (tag == null)
            {
                return;
            }

            try
            {
                Hashtable h = new Hashtable();

                hub.hub.gates.call_client(tag, "cMsgConnect", "ret_msg", h);
            }
            catch
            {

            }
        }
    }
}
