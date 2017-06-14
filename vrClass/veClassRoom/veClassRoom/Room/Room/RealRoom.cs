﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using common;
using System.Threading;

namespace veClassRoom.Room
{
    /// <summary>
    /// 真正的房间 包括大厅、场景
    /// </summary>
    class RealRoom : BaseRoomClass
    {
        public Hashtable userlist = new Hashtable();                     // 从服务器获取的当前教室的学生列表 用于登录比对

        public Dictionary<string, GroupInRoom> grouplist = new Dictionary<string, GroupInRoom>();

        // 场景的 初始化的数据   用于重置场景
        private SceneData _originalscene = new SceneData();

        ////////////////////////////////////////////////////      场景初始化、同步、销毁等操作模块      ///////////////////////////////////////////////

        public override void CreateScene(string name)
        {
            base.CreateScene(name);
            this.model = Enums.TeachingMode.WatchLearnModel_Sync;
        }

        public void CreateScene(int id)
        {
            this.sceneid = id;

            this.isinitclass = false;
            this.model = Enums.TeachingMode.WatchLearnModel_Sync;
        }

        /// <summary>
        /// 老师打开vr课件才会有的初始化场景
        /// </summary>
        /// <param name="data"></param>
        public override void InitScenes(Hashtable data)
        {
            base.InitScenes(data);

            // 初始化原始服务器数据
            this._originalscene.InitSceneData(this.moveablesceneobject, this.sceneplaylistbyid, this.sceneorderlist);

            isinitclass = true;

            Console.WriteLine("初始化服务器");
        }

        public void PlayerInitSelf3DInfor(Int64 id, Hashtable infor)
        {
            int userid = (int)id;

            if (!this.sceneplaylistbyid.ContainsKey(userid))
            {
                return;
            }

            this.sceneplaylistbyid[userid].InitPlayerHeadHand(infor);
        }

        public void BeginClass(Int64 id)
        {
            int userid = (int)id;

            Console.WriteLine("开始上课  ： " + userid + istartclass);
            if (istartclass)
            {
                return;
            }
            if (leader == null)
            {
                Console.WriteLine("老师没有进入教室 ： ");
                return;
            }

            try
            {
                if (!sceneplaylistbyid.ContainsKey(userid))
                {
                    Console.WriteLine("服务器不包含该玩家 ： " + userid);
                    return;
                }

                PlayerInScene p = sceneplaylistbyid[userid];
                if (p.selfid != leader.selfid)
                {
                    Console.WriteLine("进入教室的不是老师 无权进行开始上课的操作");
                    return;
                }

            }
            catch
            {

            }
            if (!isinitclass)
            {
                return;
            }
            // 进行一次场景同步
            SceneSynchronizationAll();
            Thread t = RoomManager.getInstance().FindThreadOfRoomById(this.sceneid);
            if (t == null)
            {
                t = RoomManager.getInstance().ApplyThreadForRoomById(this.sceneid);
            }

            if (t != null)
            {
                if (t.ThreadState == ThreadState.Unstarted)
                    t.Start();
            }

            istartclass = true;

            Console.WriteLine("开始上课  并且启动同步线程： " + userid);
        }

        public override void SceneSynchronizationAll()
        {
            base.SceneSynchronizationAll();
        }

        ////////////////////////////////////////////////////      场景初始化、同步、销毁等操作模块   End   ///////////////////////////////////////////////

        ////////////////////////////////////////////////////      向后台服务器获取玩家信息、验证登陆、反馈数据等操作模块      ///////////////////////////////////////////////

        // 获取房间所有报名者的信息
        public void GetAllPlayerInfor()
        {
            userlist = BackDataService.getInstance().GetUserList(this.scenename, null);
        }

        public void ReplyPlayerInforToService()
        {
            // 向服务器保存玩家数据
        }

        public void ReplyPlayerInforToService(UserInfor playerinfor)
        {
            // 向服务器保存玩家数据
        }

        public void ReplyPlayerInforToService(PlayerInScene player)
        {
            // 向服务器保存玩家数据
        }

        private void CheckPlayerInfor(string token, string name, string uuid)
        {
            UserInfor playerinfor = BackDataService.getInstance().CheckUser(token, name);

            if (playerinfor == null)
            {
                return;
            }

            createPlayer(playerinfor);
        }

        private void CheckPlayerInforLocal(string token, string name, string uuid)
        {
            if (userlist.Count <= 0 || !userlist.ContainsKey(token))
            {
                return;
            }

            UserInfor playerinfor = userlist[token] as UserInfor;

            createPlayer(playerinfor);
        }

        ////////////////////////////////////////////////////      向后台服务器获取玩家信息、验证登陆、反馈数据等操作模块  End    ///////////////////////////////////////////////


        ////////////////////////////////////////////////////      玩家登陆进入房间、离开、玩家创建等操作模块      ///////////////////////////////////////////////

        // 进入智慧教室
        public string PlayerEnterScene(UserInfor user)
        {
            //       CheckPlayerInfor(token, name, uuid);
            //CheckPlayerInforLocal(token, name, uuid);

            // 跳过服务器验证 只为测试
            createPlayer(user);

            if(istartclass && (user.identity != "teacher"))
            {
                SceneSynchronizationPlayer(user.uuid);
            }

            return this.scenename;
        }

        public void PlayerLeaveScene(int userid, string uuid)
        {
            PlayerInScene ps = null;
            if (sceneplaylistbyid.ContainsKey(userid))
            {
                ps = sceneplaylistbyid[userid];

                sceneplaylistbyid.Remove(userid);
            }

            if (_uuid_of_player.Contains(uuid))
            {
                _uuid_of_player.Remove(uuid);
            }

            if(sceneplaylistbyid.Count <= 0)
            {
                isactive = false;

                ClearScene();

                RoomManager.getInstance().DeleteRoomByNameById(this.sceneid);

                Console.WriteLine("清除房间 : " + this.scenename);

                return;
            }

            // 玩家离开释放 玩家锁住的物体
            if(ps != null)
            {
                foreach (ObjectInScene so in moveablesceneobject.Values)
                {
                    if (!so.locked)
                    {
                        continue;
                    }

                    if (so.locker == ps.token)
                    {
                        so.locker = null;
                        so.locked = false;
                        so.lockpermission = 0;

                        so.physical.useGravity = true;
                    }
                }
            }
        }

        private void createPlayer(UserInfor playerinfor)
        {
            if(playerinfor == null)
            {
                return;
            }

            if (playerinfor.isentercourse)
            {
                // 重复进入房间
                // TODO
                //进行场景指令同步

                return;
            }

            PlayerInScene player = new PlayerInScene(playerinfor);

            Console.WriteLine("服务器房间模式 : " + this.model);

            // 切换玩家自身模式
            if(player!=null)
            {
                player.ChangePlayerModel(this.model);
            }

            if (player.isleader)
            {
                // 登陆者是老师
                this.leader = player;
                Console.WriteLine("老师登陆 ： " + player.token);
            }else
            {
                // 登陆者是学生
            }

            int id = playerinfor.selfid;
            string uuid = playerinfor.uuid;
            if (this.sceneplaylistbyid.ContainsKey(id))
            {
                // 重复登陆  可能是掉线重登
                //TODO
                PlayerInScene p = this.sceneplaylistbyid[id];
                if (_uuid_of_player.Contains(p.uuid))
                {
                    _uuid_of_player.Remove(p.uuid);
                }

                _uuid_of_player.Add(uuid);

                this.sceneplaylistbyid[id] = player;

                Console.WriteLine("用户重复登陆 : " + player.name);
            }
            else
            {
                this.sceneplaylistbyid.Add(id, player);

                if (!_uuid_of_player.Contains(uuid))
                {
                    _uuid_of_player.Add(uuid);
                }
            }
            Console.WriteLine("当前玩家 id : " + id);
            Console.WriteLine("当前玩家数 : " + sceneplaylistbyid.Count + " _uuid_of_player " + _uuid_of_player.Count);

            // 通知客户端 玩家进入 教学大厅

            //// 通知客户端显示登陆信息
            //string msg = playerinfor.user_name + "登陆进入房间";
            //TellClientMsg(this._uuid_of_player, msg);
        }

        ////////////////////////////////////////////////////      玩家登陆进入房间、离开、玩家创建等操作模块   End   ///////////////////////////////////////////////


        ////////////////////////////////////////////////////      具体和客户端的操作      ///////////////////////////////////////////////

        public override void Switch_Model(int userid, Int64 tomodel, string uuid)
        {
            if(this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            if(sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(userid))
            {
                Console.WriteLine("服务器玩家不存在 : " + " sceneplaylist count : " + sceneplaylistbyid.Count);
                return;
            }

            if(!checkLeaderFeasible(sceneplaylistbyid[userid]))
            {
                // 权限不够
                Console.WriteLine("切换模式权限不够 : " + " tomodel: " + tomodel);
                return;
            }

            Console.WriteLine("切换模式成功 : " + " tomodel: " + tomodel + "  开始更改玩家状态");

            Enums.TeachingMode m = (Enums.TeachingMode)(tomodel);

            if (m != this.model)
            {
                this.model = m;

                try
                {
                    foreach (PlayerInScene player in sceneplaylistbyid.Values)
                    {
                        player.ChangePlayerModel(this.model);
                    }
                }
                catch
                {

                }

            }

            //if (_uuid_of_player.Count > 0)
            //{
            //    hub.hub.gates.call_group_client(_uuid_of_player, "cMsgConnect", "ret_change_one_model", userid, tomodel);
            //}

            Console.WriteLine("更改了当前房间所有玩家模式成功 " + " tomodel: " + tomodel);
        }

        public override void Req_Object_Operate_permissions(Int64 id, string objectname, string uuid)
        {
            int userid = (int)id;

            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            PlayerInScene ps = findPlayerById(userid);
            if(ps == null)
            {
                return;
            }

            // 先判断是否有小组
            if(ps.group != null)
            {
                ps.group.Req_Object_Operate_permissions(userid, objectname, uuid);

                return;
            }

            // 没有小组执行以下操作

            //具体的操作控制
            bool ret = false;

            do
            {
                if (!(checkOperateFeasible(ps) || ps.isCanOperate))
                {
                    Console.WriteLine("Req_Object_Operate_permissions 1 无权操作 : " + "token : " + userid);
                    break;
                }

                if (!moveablesceneobject.ContainsKey(objectname))
                {
                    Console.WriteLine("token : " + userid + "请求操作物体" + objectname + "失败 因为服务器不存在该物体");
                    break;
                }

                ObjectInScene o = moveablesceneobject[objectname];
                if (o.locked)
                {
                    if (o.locker == ps.token)
                    {
                        Console.WriteLine("token : " + ps.token + "重复请求操作物体" + objectname);
                        break;
                    }

                    if(o.lockpermission < ps.permission)
                    {
                        o.locker = ps.token;
                        o.locked = true;
                        o.lockpermission = ps.permission;

                        o.physical.useGravity = false;

                        ret = true;

                        Console.WriteLine("token : " + ps.token + "权限较高夺取了 token  " + o.locker + " 的物体 " + objectname);
                        break;
                    }

                    break;
                }
                else
                {
                    o.locker = ps.token;
                    o.locked = true;
                    o.lockpermission = ps.permission;

                    o.physical.useGravity = false;

                    ret = true;
                }

            } while (false);

            if(_uuid_of_player.Count > 0)
            {
                hub.hub.gates.call_group_client(_uuid_of_player, "cMsgConnect", "ret_operation_permissions", userid, objectname, "hold", ret ? "yes" : "no");
            }
        }

        public override void Req_Object_Release_permissions(Int64 id, string objectname, string uuid)
        {
            int userid = (int)id;

            Console.WriteLine("Req_Object_Release_permissions uuid " + uuid + objectname);

            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            PlayerInScene ps = findPlayerById(userid);
            if (ps == null)
            {
                return;
            }

            // 先判断是否有小组
            if (ps.group != null)
            {
                ps.group.Req_Object_Release_permissions(userid, objectname, uuid);

                return;
            }

            // 没有小组执行以下操作

            //具体的操作控制
            bool ret = false;
            do
            {
                if (!(checkOperateFeasible(ps) || ps.isCanOperate))
                {
                    Console.WriteLine("Req_Object_Release_permissions 1 无权操作 : " + "token : " + userid);
                    break;
                }

                if (!moveablesceneobject.ContainsKey(objectname))
                {
                    Console.WriteLine("token : " + userid + "请求释放物体" + objectname + "操作权限失败 因为服务器不存在该物体");
                    break;
                }

                ObjectInScene o = moveablesceneobject[objectname];
                if (!o.locked)
                {
                    Console.WriteLine("token : " + userid + "请求释放物体" + objectname + "操作权限失败 因为服务器该物体锁已被释放");
                    break;
                }
                else if (o.locker != ps.token)
                {
                    Console.WriteLine("token : " + ps.token + "请求释放物体" + objectname + "操作权限失败 因为服务器该物体被 token : " + o.locker + "锁住");
                    break;
                }
                else
                {
                    o.locker = null;
                    o.locked = false;
                    o.lockpermission = Enums.PermissionEnum.None;

                    // 设置重力
                    o.physical.useGravity = true;

                    ret = true;
                }

            } while (false);

            if(_uuid_of_player.Count > 0)
            {
                hub.hub.gates.call_group_client(_uuid_of_player, "cMsgConnect", "ret_operation_permissions", userid, objectname, "release", ret ? "yes" : "no");
            }
        }

        public override void ChangeObjectAllOnce(Int64 userid, Hashtable clientallonce)
        {
            int id = (int)userid;

            Console.WriteLine("ChangeObjectAllOnce uuid " + id + clientallonce.Count);

            if (clientallonce == null || clientallonce.Count <= 0)
            {
                return;
            }

            PlayerInScene ps = findPlayerById(id);
            if (ps == null)
            {
                return;
            }

            // 先判断是否有小组
            if (ps.group != null)
            {
                ps.group.ChangeObjectAllOnce(id, clientallonce);

                return;
            }

            // 没有小组执行以下操作

            if (!(checkOperateEffective(ps) || ps.isCanSend))
            {
                return;
            }

            do
            {
                string token = ps.token;

                foreach (DictionaryEntry de in clientallonce)
                {
                    if (!moveablesceneobject.ContainsKey((string)de.Key))
                    {
                        // 服务器不包含该物体
                        continue;
                    }

                    var o = moveablesceneobject[(string)de.Key];

                    if (!o.locked || o.locker == null)
                    {
                        continue;
                    }

                    if (!(o.locker == token || o.lockpermission < ps.permission))
                    {
                        continue;
                    }

                    o.Conversion((Hashtable)de.Value);
                }

            } while (false);
        }

        public override void ret_sync_commond(string typ, string commond, Int64 id, string other, string uuid)
        {
            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            int userid = (int)id;

            if (sceneplaylistbyid.Count <= 0|| !sceneplaylistbyid.ContainsKey(userid))
            {
                return;
            }

            if (_uuid_of_player == null || _uuid_of_player.Count <= 0)
            {
                return;
            }

            try
            {
                _uuid_of_player.Remove(uuid);
                hub.hub.gates.call_group_client(_uuid_of_player, "cMsgConnect", "ret_sync_commond", typ, commond, userid, other);
                _uuid_of_player.Add(uuid);
            }
            catch
            {

            }

            // 指令缓存
            sceneorderlist.Add(new OrderInScene(0, userid, typ,commond,other));  //后期加入时间机制
        }

        public void ret_sync_group_commond(string typ, string commond, Int64 id, string other, string uuid)
        {
            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            int userid = (int)id;

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(userid))
            {
                return;
            }

            PlayerInScene ps = findPlayerById(userid);
            if (ps == null)
            {
                return;
            }

            // 先判断是否有小组
            if (ps.group != null)
            {
                ps.group.ret_sync_commond(typ, commond, userid, other, uuid);

                return;
            }

            // 没有小组执行以下操作

            ret_sync_commond(typ, commond, userid, other, uuid);
        }

        public override void SyncClient()
        {
            Hashtable msgObject = new Hashtable();
            Hashtable msgPlayer = new Hashtable();
            while (_syncstate)
            {
                // 同步数据
                foreach (ObjectInScene so in moveablesceneobject.Values)
                {
                    if (!so.changeorno)
                    {
                        continue;
                    }

                    // 同步客户端
                    msgObject.Add(so.name, so.Serialize());
                }

                foreach (PlayerInScene sp in sceneplaylistbyid.Values)
                {
                    if (!sp.changeorno)
                    {
                        continue;
                    }

                    // 同步客户端
                    msgPlayer.Add(sp.name, sp.Serialize());
                }

                // 同步指令
                //TODO

                // 同步客户端
                if (msgPlayer.Count > 0 || msgObject.Count > 0)
                {
                    try
                    {

                        if (_uuid_sync_cache.Count > 0)
                        {
                            _uuid_sync_cache.Clear();
                        }

                        foreach (PlayerInScene p in sceneplaylistbyid.Values)
                        {
                            if (p.isCanReceive)
                            {
                                Console.WriteLine("同步客户端名字 : " + p.name);
                                _uuid_sync_cache.Add(p.uuid);
                            }
                        }

                    }
                    catch
                    {

                    }

                    if (_uuid_sync_cache.Count > 0)
                    {
                        hub.hub.gates.call_group_client(_uuid_sync_cache, "cMsgConnect", "SyncClient", msgObject, msgPlayer);

                        Console.WriteLine("同步客户端数据 _uuid_sync_cache : " + _uuid_sync_cache.Count);

                        _uuid_sync_cache.Clear();
                    }
                }

                // 同步之后清除
                msgObject.Clear();
                msgPlayer.Clear();

                Thread.Sleep(16);
            }
        }


        // 动态修改 玩家 权限 等操作
        // TODO
        // 更改某一玩家的模式状态
        public void Change_One_Model(Int64 id, Int64 tomodel, string uuid, Int64 target)
        {
            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            int userid = (int)id;
            int targetid = (int)target;

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(userid))
            {
                return;
            }

            if(!sceneplaylistbyid.ContainsKey(targetid))
            {
                return;
            }

            if (!checkLeaderFeasible(sceneplaylistbyid[userid]))
            {
                // 权限不够
                return;
            }

            PlayerInScene ps = sceneplaylistbyid[targetid];
            this.model = (Enums.TeachingMode)(tomodel);
            ps.ChangePlayerModel(this.model);

            hub.hub.gates.call_client(ps.uuid, "cMsgConnect", "ret_change_one_model", targetid, tomodel);
        }

        // 更改某些玩家的模式状态
        public void Change_Some_Model(Int64 id, Int64 tomodel, ArrayList someid)
        {
            int userid = (int)id;

            if (someid == null || someid.Count <= 0)
            {
                return;
            }

            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(userid))
            {
                return;
            }

            if (!checkLeaderFeasible(sceneplaylistbyid[userid]))
            {
                // 权限不够
                return;
            }

            this.model = (Enums.TeachingMode)(tomodel);

            ArrayList uuidcache = new ArrayList();
            foreach(int t in someid)
            {
                if(!sceneplaylistbyid.ContainsKey(t))
                {
                    continue;
                }

                PlayerInScene ps = sceneplaylistbyid[t];
                ps.ChangePlayerModel(this.model);

                uuidcache.Add(ps.uuid);
            }

            if(uuidcache.Count > 0)
            {
                hub.hub.gates.call_group_client(uuidcache, "cMsgConnect", "ret_change_one_model", userid, tomodel);
            }

            uuidcache.Clear();
        }


        ////////////////////////////////////////////////////   小组 管理相关     ///////////////////////////////////////////////////////////////////////////////////
        // 分组函数
        private void divideGroupByGrade()
        {
            int count = sceneplaylistbyid.Count;
            int groupcount = count / 10;
            int remainder = count % 10;
            if(remainder > 0)
            {
                groupcount++;
            }
            int membercount = count / groupcount;
            remainder = count % groupcount; // 多余的人数 插入到 前几组之中

            string groupname = "group";
            for(int i=0;i<groupcount;i++)
            {
                GroupInRoom gir = new GroupInRoom();
                if(remainder > 0)
                {
                    gir.playercount = membercount + 1;
                    remainder--;
                }
                else
                {
                    gir.playercount = membercount;
                }
                grouplist.Add(groupname + i, gir);
            }

            remainder = 0;
            membercount = 0;
            GroupInRoom girr = grouplist[groupname + remainder];
            foreach (PlayerInScene ps in sceneplaylistbyid.Values)
            {
                if(girr == null)
                {
                    break;
                }

                girr.AddMember(ps);
                ps.group = girr;
                membercount++;

                if (membercount >= girr.playercount)
                {
                    remainder++;
                    membercount = 0;
                    girr = grouplist[groupname + remainder];
                }
            }
        }

        private Hashtable divideGroupOnAverage()
        {
            Hashtable players = new Hashtable();

            int count = sceneplaylistbyid.Count;
            //int groupcount = count / 10;
            //int remainder = count % 10;
            //if (remainder > 0)
            //{
            //    groupcount++;
            //}
            //int membercount = count / groupcount;
            //remainder = count % groupcount; // 多余的人数 插入到 前几组之中

            //string groupname = "group";
            //for (int i = 0; i < groupcount; i++)
            //{
            //    GroupInRoom gir = new GroupInRoom(groupname + i);
            //    if (remainder > 0)
            //    {
            //        gir.playercount = membercount + 1;
            //        remainder--;
            //    }
            //    else
            //    {
            //        gir.playercount = membercount;
            //    }
            //    grouplist.Add(groupname + i, gir);
            //}

            //remainder = 0;
            //membercount = 0;
            //GroupInRoom girr = grouplist[groupname + remainder];
            //foreach (PlayerInScene ps in sceneplaylistbyid.Values)
            //{
            //    if (girr == null)
            //    {
            //        break;
            //    }

            //    girr.AddMember(ps);
            //    ps.group = girr;
            //    membercount++;

            //    players.Add(ps.selfid, girr.name);

            //    if (membercount >= girr.playercount)
            //    {
            //        remainder++;
            //        membercount = 0;
            //        girr = grouplist[groupname + remainder];
            //    }
            //}

            // 测试
            if(count <= 10)
            {
                GroupInRoom girr = new GroupInRoom("group1");
                foreach (PlayerInScene ps in sceneplaylistbyid.Values)
                {
                    if (girr == null)
                    {
                        break;
                    }

                    if(ps.selfid == this.leader.selfid)
                    {
                        continue;
                    }

                    girr.AddMember(ps);
                    ps.group = girr;

                    players.Add(ps.selfid, girr.name);
                }

                grouplist.Add("group1", girr);
            }

            return players;
        }

        public void DivideGroup(Int64 id, string rules)
        {
            int userid = (int)id;

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(userid))
            {
                Console.WriteLine("服务器玩家不存在 : " + "userid : " + userid + " sceneplaylist count : " + sceneplaylistbyid.Count);
                return;
            }

            if(userid != this.leader.selfid)
            {
                // 权限不够
                Console.WriteLine("切换模式权限不够 不是老师不可分组 : " + "userid : " + userid);
                return;
            }

            // 按成绩分组
            //        divideGroupByGrade();
            // 平均分组
            Hashtable players = divideGroupOnAverage();
            if(this.leader != null)
            {
                hub.hub.gates.call_client(this.leader.uuid, "cMsgConnect", "retDivideGroup", userid, players);
            }
        }

        /// 选择某一学生 或者某一小组 操作 全班同学观看
        /// 仅限于观学模式
        public void ChooseOneOrGroupOperate(Int64 id, string groupname, bool isgroup = false)
        {
            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            int userid = (int)id;

            // 当前模式必须是   指导模式才行
            if (this.model != Enums.TeachingMode.WatchLearnModel_Async || this.model != Enums.TeachingMode.WatchLearnModel_Sync)
            {
                return;
            }

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(userid))
            {
                return;
            }

            if (!checkLeaderFeasible(sceneplaylistbyid[userid]))
            {
                // 权限不够
                return;
            }

            if(isgroup)
            {
                if(grouplist.Count <= 0 || !grouplist.ContainsKey(groupname))
                {
                    Console.WriteLine("当前房间不存在该小组 : " + groupname);
                    return;
                }

                // 组内协作 并且广播 给 所有玩家 
                // TODO
                // 1,改变 组内成员权限 可操作 可同步
                GroupInRoom gir = grouplist[groupname];
                if(gir == null)
                {
                    return;
                }

  //              gir.SwitchModelTo(Enums.ModelEnums.Collaboration);
                // 2,向所选择小组注入观看成员
                gir.InjectiveViewer(_uuid_of_player);

                // 通知该小组成员 被选中
   ///             gir.TellPlayerBeSelected(token, name);
            }
            else
            {
                // 改变一个人的权限 并且广播
                int targetid = Convert.ToInt32(groupname);
                if (!sceneplaylistbyid.ContainsKey(targetid))
                {
                    return;
                }

                // 学生可操作 并且广播给所有学生 但是 老师可介入
                //TODO
                // 1,特殊改变某一学生的操作开关
                PlayerInScene ps = sceneplaylistbyid[targetid];
                if(ps == null)
                {
                    return;
                }

                ps.ChangePlayerCanOperate(this.leader.permission, true);
                ps.ChangePlayerCanSend(this.leader.permission, true);
                ps.ChangePlayerCanReceive(this.permission, true);

                // 通知学生他被选中
                hub.hub.gates.call_client(ps.uuid, "cMsgConnect", "ret_choose_one_operate", userid, groupname);
            }
        }

        /////////////////////////////////////////   和客户端交互需要用到的 辅助函数  （私有）     //////////////////////////////////////////////////////////////////////////////
        // TODO


        ////////////////////////////////////////     具体和客户端的界面操作有关的 rpc函数   与UI界面 交互接口      /////////////////////////////////////////////////////////////

        /// <summary>
        /// 教学模式切换操作
        /// </summary>
        /// <param name="token">操作者token</param>
        /// <param name="mode">切换的教学模式 TeachingMode 对应教学5大模式：观学模式、指导模式，自主训练、视频点播、视频直播</param>
        /// <param name="target">对应操作类型的操作对象：个人、组、、全部、空</param>
        public void SwitchTeachMode(Int64 userid, Int64 mode, Int64 isgroup, string target = null)
        {
            Console.WriteLine("客户端切换模式 UI " + "mode" + mode + "target" + target);

            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            int id = (int)userid;

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(id))
            {
                Console.WriteLine("UI 服务器玩家不存在 : " + " sceneplaylist count : " + sceneplaylistbyid.Count);
                return;
            }

            if (!checkLeaderFeasible(sceneplaylistbyid[id]))
            {
                // 权限不够
                Console.WriteLine("UI 教学模式切换操作 权限不够 : " + "userid : " + userid);
                return;
            }

            Enums.TeachingMode tm = (Enums.TeachingMode)mode;

            switch(tm)
            {
                case Enums.TeachingMode.WatchLearnModel_Sync:
                   // break;
                case Enums.TeachingMode.WatchLearnModel_Async:
                    this.model = tm;
                    try
                    {
                        foreach (PlayerInScene player in sceneplaylistbyid.Values)
                        {
                            player.ChangePlayerModel(this.model);
                        }
                    }
                    catch
                    {

                    }
                    break;
                case Enums.TeachingMode.GuidanceMode_Personal:
                    if (target == null)
                    {
                        break;
                    }
                    int targetuserid = Convert.ToInt32(target);
                    if(!sceneplaylistbyid.ContainsKey(targetuserid))
                    {
                        Console.WriteLine("GuidanceMode_Personal UI 服务器玩家不存在 : " + "target : " + target);
                        return;
                    }
                    this.model = tm;
                    try
                    {
                        foreach (PlayerInScene player in sceneplaylistbyid.Values)
                        {
                            player.ChangePlayerModel(this.model);
                            if (player.selfid == targetuserid)
                            {
                                player.ChangePlayerCanSend(this.leader.permission, true);
                                player.ChangePlayerCanReceive(this.leader.permission, true);
                                player.ChangePlayerCanOperate(this.leader.permission, true);
                                player.isbechoosed = true;
                            }
                            else
                            {
                                player.isbechoosed = false;
                            }
                        }
                    }
                    catch
                    {

                    }
                    break;
                case Enums.TeachingMode.GuidanceMode_Group:
                    if (target == null)
                    {
                        break;
                    }
                    if (grouplist.Count <= 0 || !grouplist.ContainsKey(target))
                    {
                        Console.WriteLine("GuidanceMode_Personal UI 服务器玩家不存在 : " + "target : " + target);
                        return;
                    }
                    this.model = tm;
                    GroupInRoom gir = grouplist[target];
                    try
                    {
                        foreach (PlayerInScene player in sceneplaylistbyid.Values)
                        {
                            if(!gir.HasMember(player.token))
                            {
                                player.ChangePlayerModel(this.model);
                                gir.InjectiveViewer(player.token);
                            }
                            else
                            {
                                player.ChangePlayerModel(this.model);
                                player.ChangePlayerCanSend(this.leader.permission, true);
                                player.ChangePlayerCanReceive(this.leader.permission, true);
                                player.ChangePlayerCanOperate(this.leader.permission, true);
                            }
                        }
                    }
                    catch
                    {

                    }
                    break;
                case Enums.TeachingMode.SelfTrain_Personal:
                    if (target == null)
                    {
                        break;
                    }
                    int tuserid = (int)(userid);
                    if (!sceneplaylistbyid.ContainsKey(tuserid))
                    {
                        Console.WriteLine("SelfTrain_Personal UI 服务器玩家不存在 : " + "target : " + target);
                        return;
                    }
                    this.model = tm;
                    try
                    {
                        foreach (PlayerInScene player in sceneplaylistbyid.Values)
                        {
                            player.ChangePlayerModel(this.model);
                        }
                    }
                    catch
                    {

                    }
                    break;
                case Enums.TeachingMode.SelfTrain_Group:
                    this.model = tm;
                    try
                    {
                        foreach (PlayerInScene player in sceneplaylistbyid.Values)
                        {
                            player.ChangePlayerModel(this.model);
                        }
                    }
                    catch
                    {

                    }
                    break;
                case Enums.TeachingMode.SelfTrain_All:
                    this.model = tm;
                    try
                    {
                        foreach (PlayerInScene player in sceneplaylistbyid.Values)
                        {
                            player.ChangePlayerModel(this.model);
                        }
                    }
                    catch
                    {

                    }
                    break;
                case Enums.TeachingMode.VideoOnDemand_General:
                    break;
                case Enums.TeachingMode.VideoOnDemand_Full:
                    break;
                case Enums.TeachingMode.VideoOnLive_General:
                    break;
                case Enums.TeachingMode.VideoOnLive_Full:
                    break;
                default:
                    break;
            }

            // 回复客户端
            if (_uuid_of_player.Count > 0)
            {
                hub.hub.gates.call_group_client(_uuid_of_player, "cMsgConnect", "retSwitchTeachMode", userid, mode, target);
            }
        }

        // 获取题目数据
        public void AcquireQuestionList(Int64 userid)
        {
            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            int id = (int)userid;

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(id))
            {
                return;
            }

            if (!checkLeaderFeasible(sceneplaylistbyid[id]))
            {
                // 权限不够
                Console.WriteLine("UI 获取题目列表 权限不够 : " + "userid : " + userid);
                return;
            }

            BackDataService.getInstance().GetCourseQuestionList(sceneplaylistbyid[id].token, Question_List_Succeed, Question_List_Failure, userid.ToString());

            
        }

        public void Question_List_Succeed(BackDataType.QuestionInforRetData v, string jsondata, string tag, string url)
        {
            if (tag == null)
            {
                return;
            }

            try
            {
                int id = Convert.ToInt32(tag);
                if (!sceneplaylistbyid.ContainsKey(id))
                {
                    return;
                }

                PlayerInScene user = sceneplaylistbyid[id];

                // 转换编码格式
                jsondata = JsonDataHelp.getInstance().EncodeBase64(null, jsondata);

                // 广播 所有学生 测验题信息
                if (_uuid_of_player.Count > 0)
                {
                    hub.hub.gates.call_group_client(_uuid_of_player, "cMsgConnect", "retAcquireQuestionList", id, jsondata);
                }
            }
            catch
            {

            }
        }

        public void Question_List_Failure(BackDataType.MessageRetHead errormsg, string tag)
        {
            if (tag == null)
            {
                return;
            }

            try
            {
                int id = Convert.ToInt32(tag);
                if (!sceneplaylistbyid.ContainsKey(id))
                {
                    return;
                }

                PlayerInScene user = sceneplaylistbyid[id];

                hub.hub.gates.call_client(user.uuid, "cMsgConnect", "retAcquireQuestionList", id, "null");
            }
            catch
            {

            }
        }

        /// <summary>
        /// 老师随堂测验的操作， 记得区分 在vr课件里 还是 在 学习大厅
        /// </summary>
        /// <param name="token">操作者token</param>
        /// <param name="typ">随堂测验的类型</param>
        /// <param name="question">具体问题信息id</param>
        /// <param name="other">额外的参数信息</param>
        public void InClassTest(Int64 userid, Int64 typ, Int64 questionid, string other = null)
        {
            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            int id = (int)userid;

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(id))
            {
                return;
            }

            if (!checkLeaderFeasible(sceneplaylistbyid[id]))
            {
                // 权限不够
                return;
            }

            //Enums.InClassTestType ctt = (Enums.InClassTestType)typ;

            //switch(ctt)
            //{
            //    case Enums.InClassTestType.Test:
            //        break;
            //    case Enums.InClassTestType.Fast:
            //        break;
            //    case Enums.InClassTestType.Ask:
            //        break;
            //    default:
            //        break;
            //}

            // 初始化 对应的 class类

            // 广播 所有学生 测验题
            if (_uuid_of_player.Count > 0)
            {
                hub.hub.gates.call_group_client(_uuid_of_player, "cMsgConnect", "retInClassTest", userid, typ, questionid, other);
            }
        }

        /// <summary>
        /// 重置场景相关
        /// </summary>
        /// <param name="token">操作者token</param>
        /// <param name="typ">重置对象类型：全部、组、个人</param>
        /// <param name="target">具体重置对象信息：名字</param>
        public void ResetScene(Int64 userid, Int64 typ, string target)
        {
            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            int id = (int)userid;

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(id))
            {
                return;
            }

            if (!checkLeaderFeasible(sceneplaylistbyid[id]))
            {
                // 权限不够
                Console.WriteLine("UI 教学模式切换操作 权限不够 : " + "userid : " + userid);
                return;
            }

            Enums.OperatingRange or = (Enums.OperatingRange)typ;

            switch(or)
            {
                case Enums.OperatingRange.Personal:
                    if(target == null)
                    {
                        return;
                    }
                    int targetid = Convert.ToInt32(target);

                    if (!sceneplaylistbyid.ContainsKey(targetid))
                    {
                        Console.WriteLine("UI 服务器玩家不存在 : " + "token : " + target);
                        return;
                    }
                    PlayerInScene ps = sceneplaylistbyid[targetid];
                    hub.hub.gates.call_client(ps.uuid, "cMsgConnect", "retResetScene", userid);
                    break;
                case Enums.OperatingRange.Team:
                    if (target == null)
                    {
                        return;
                    }

                    if (!grouplist.ContainsKey(target))
                    {
                        Console.WriteLine("UI 服务器小组不存在 : " + "token : " + target);
                        return;
                    }
                    GroupInRoom gir = grouplist[target];
                    hub.hub.gates.call_group_client(gir._uuid_of_player, "cMsgConnect", "retResetScene", userid);
                    break;
                case Enums.OperatingRange.All:
                    if(_uuid_of_player.Count > 0)
                    {
                        hub.hub.gates.call_group_client(this._uuid_of_player, "cMsgConnect", "retResetScene", userid);
                    }
                    break;
                default:
                    break;
            }
        }


        ////////////////////////////////////////     具体和学生互动有关的 rpc函数  学生点赞、举手  老师  接收答题反馈 等  /////////////////////////////////////
        // 学生答题反馈
        public void AnswerQuestion(Int64 userid, Int64 questionid, Int64 optionid)
        {
            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            int id = (int)userid;

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(id))
            {
                return;
            }

            hub.hub.gates.call_client(this.leader.uuid, "cMsgConnect", "retAnswerQuestion", userid, questionid, optionid);
        }
        // 学生抢答反馈
        public void AnswerFastQuestion(Int64 userid, Int64 questionid)
        {
            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            int id = (int)userid;

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(id))
            {
                return;
            }

            hub.hub.gates.call_client(this.leader.uuid, "cMsgConnect", "retAnswerFastQuestion", userid, questionid);
        }
        // 点赞
        public void SendLikeToTeacher(Int64 userid)
        {
            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            int id = (int)userid;

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(id))
            {
                return;
            }

            hub.hub.gates.call_client(this.leader.uuid, "cMsgConnect", "retSendLike", userid);
        }

        // 举手
        public void SendDoubtToTeacher(Int64 userid)
        {
            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            int id = (int)userid;

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(id))
            {
                return;
            }

            hub.hub.gates.call_client(this.leader.uuid, "cMsgConnect", "retSendDoubt", userid);
        }

        // 推送电子白板
        public void SwitchWhiteBoard(Int64 userid, Int64 openclose)
        {
            if (this.leader == null || this.leader.uuid == null)
            {
                return;
            }

            int id = (int)userid;

            if (sceneplaylistbyid.Count <= 0 || !sceneplaylistbyid.ContainsKey(id))
            {
                return;
            }

            hub.hub.gates.call_client(this.leader.uuid, "cMsgConnect", "retWhiteBoard", userid, openclose);
        }

        // 返回大厅
        public void BackToLobby(Int64 userid)
        {
            hub.hub.gates.call_client(this.leader.uuid, "cMsgConnect", "retBackToLobby", userid);
        }

    }
}
