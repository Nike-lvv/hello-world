﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WisdomClassroom.ClassRoom
{
    class BackDataService
    {
        public static BackDataService getInstance()
        {
            return Singleton<BackDataService>.getInstance();
        }

        public void CheckUser(string name, string password, DataRetCallBackSucceed<BackDataType.PlayerLoginRetData> onSucceed, DataRetCallBackFailure onFailure, string tag = null)
        {
            if (tag == null)
            {
                tag = name;
            }

            CheckPlayerLogin(name, password, onSucceed, onFailure, tag);
        }

        public Hashtable GetUserList(string token, params Object[] p)
        {
            return null;
        }

        public Hashtable GetUserTokenNameList(string token)
        {
            Hashtable h = new Hashtable();
            h.Add("student1", "若雨");
            h.Add("student2", "寒露");

            return h;
        }

        // 和后台服务器的接口/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //public static string get_uft8(string unicodeString)
        //{
        //    UTF8Encoding utf8 = new UTF8Encoding();
        //    Byte[] encodedBytes = utf8.GetBytes(unicodeString);
        //    String decodedString = utf8.GetString(encodedBytes);

        //    Console.WriteLine(decodedString);

        //    return decodedString;
        //}

        // 登陆验证 即获取token等信息
        // login 返回数据 {"message":"登录成功","code":0,"type":"","data":{"id":"5","name":"lixin","access_token":"uWBpifKV2D9p6UlHpXhrkf3zP_1X1MPc"}}
        private readonly string url_login = BackServerConfig.HD_Url + "/api-v1/login.html";
        public void CheckPlayerLogin(string username, string password, DataRetCallBackSucceed<BackDataType.PlayerLoginRetData> onSucceed = null, DataRetCallBackFailure onFailure = null, string tag = null)
        {
            if (username == null || password == null)
            {
                return;
            }

            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("username", username);
            parameters.Add("password", password);

            AsyncGetJsonData<BackDataType.PlayerLoginRetData> agjd = new AsyncGetJsonData<BackDataType.PlayerLoginRetData>();
            agjd.onSucceed = onSucceed;
            agjd.onFailure = onFailure;

            if (tag == null)
            {
                tag = username;
            }
            agjd.GetJsonData(url_login, tag, AsyncGetJsonData<BackDataType.PlayerLoginRetData>.GetPost.Post, parameters);

        }

        // 获取学生基本信息
        private readonly string url_getplayerbaseinfor = BackServerConfig.HD_Url + "/api-v1/user.html?access-token=";
        public void GetPlayerBaseInfor(string accesstoken, DataRetCallBackSucceed<BackDataType.PlayerBaseInforRetData> onSucceed = null, DataRetCallBackFailure onFailure = null, string tag = null)
        {
            if (accesstoken == null)
            {
                return;
            }

            string url = url_getplayerbaseinfor + accesstoken;

            AsyncGetJsonData<BackDataType.PlayerBaseInforRetData> agjd = new AsyncGetJsonData<BackDataType.PlayerBaseInforRetData>();
            agjd.onSucceed = onSucceed;
            agjd.onFailure = onFailure;

            if (tag == null)
            {
                tag = accesstoken;
            }

            agjd.GetJsonData(url, tag);

        }

        // 获取学生VR课程列表
        private readonly string url_student_courses = BackServerConfig.HD_Url + "/api-v1/course/study.html?access-token={0}&mode=expe";
        public void GetStudentCourseList(string accesstoken, DataRetCallBackSucceed<BackDataType.CourseListRetData> onSucceed = null, DataRetCallBackFailure onFailure = null, string tag = null)
        {
            if (accesstoken == null)
            {
                return;
            }

            string url = string.Format(url_student_courses, accesstoken);

            AsyncGetJsonData<BackDataType.CourseListRetData> agjd = new AsyncGetJsonData<BackDataType.CourseListRetData>();
            agjd.onSucceed = onSucceed;
            agjd.onFailure = onFailure;
            if (tag == null)
            {
                tag = accesstoken;
            }
            agjd.GetJsonData(url, tag);

        }

        // 获取老师创建课程列表
        private readonly string url_teacher_courses = BackServerConfig.HD_Url + "/api-v1/course/teacher.html?access-token=";
        private readonly string course_mode = "&mode=";
        public void GetTeacherCourseList(string accesstoken, string coursemode, DataRetCallBackSucceed<BackDataType.CourseListRetData> onSucceed = null, DataRetCallBackFailure onFailure = null, string tag = null)
        {
            if (accesstoken == null)
            {
                return;
            }

            string url = url_teacher_courses + accesstoken;
            if (coursemode != null)
            {
                url += course_mode + coursemode;
            }

            AsyncGetJsonData<BackDataType.CourseListRetData> agjd = new AsyncGetJsonData<BackDataType.CourseListRetData>();
            agjd.onSucceed = onSucceed;
            agjd.onFailure = onFailure;

            if (tag == null)
            {
                tag = accesstoken;
            }

            agjd.GetJsonData(url, tag);

        }
        //获取某个课程下有哪些学生
        private readonly string url_course_student = BackServerConfig.HD_Url + "/api-v1/course/students.html?access-token={0}&id={1}";
        public void GetCourseStudentList(string accesstoken, string classid, DataRetCallBackSucceed<BackDataType.CourseInforRetData> onSucceed = null, DataRetCallBackFailure onFailure = null, string tag = null)
        {
            if (accesstoken == null)
            {
                return;
            }

            string url = string.Format(url_course_student, accesstoken, classid);

            AsyncGetJsonData<BackDataType.CourseInforRetData> agjd = new AsyncGetJsonData<BackDataType.CourseInforRetData>();
            agjd.onSucceed = onSucceed;
            agjd.onFailure = onFailure;

            if (tag == null)
            {
                tag = accesstoken;
            }
            agjd.GetJsonData(url, tag);

        }
        // 获取课程题目数据
        private readonly string url_question_get = BackServerConfig.HD_Url + "/api-v1/question/course.html?access-token={0}&id=xxx";
        public void GetCourseQuestionList(string accesstoken, DataRetCallBackSucceed<BackDataType.QuestionInforRetData> onSucceed, DataRetCallBackFailure onFailure, string tag = null)
        {
            if (accesstoken == null)
            {
                return;
            }

            string url = string.Format(url_question_get, accesstoken);

            AsyncGetJsonData<BackDataType.QuestionInforRetData> agjd = new AsyncGetJsonData<BackDataType.QuestionInforRetData>();
            agjd.onSucceed = onSucceed;
            agjd.onFailure = onFailure;

            if (tag == null)
            {
                tag = accesstoken;
            }

            agjd.GetJsonData(url, tag);

        }
        // 获取课程资料列表
        private readonly string url_material_get = BackServerConfig.HD_Url + "/api-v1/course/materials.html?access-token={0}&id={1}";
        public void GetCourseMaterialList(string accesstoken, int courseid = -1, DataRetCallBackSucceed<BackDataType.MaterialItemInforRetData> onSucceed = null, DataRetCallBackFailure onFailure = null, string tag = null)
        {
            if (accesstoken == null || courseid < 0)
            {
                return;
            }

            string url = string.Format(url_material_get, accesstoken, courseid);

            Console.WriteLine(url);

            AsyncGetJsonData<BackDataType.MaterialItemInforRetData> agjd = new AsyncGetJsonData<BackDataType.MaterialItemInforRetData>();
            agjd.onSucceed = onSucceed;
            agjd.onFailure = onFailure;

            if (tag == null)
            {
                tag = accesstoken;
            }

            agjd.GetJsonData(url, tag);

        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 解析成功返回
        /// </summary>
        /// <typeparam name="T">解析的目标对象</typeparam>
        /// <param name="data">解析后的目标对象数据</param>
        /// <param name="jsondata">原始json数据</param>
        /// <param name="tag">调用者标记</param>
        public delegate void DataRetCallBackSucceed<T>(T data, string jsondata, string tag = null, string url = null);
        public delegate void DataRetCallBackFailure(BackDataType.MessageRetHead msg, string tag = null);

        public class AsyncGetJsonData<T>
        {
            public enum GetPost
            {
                Get = 1,
                Post,
            }

            Thread recive;

            string url;
            IDictionary<string, string> parameters;
            string tag; // 回掉的标记 表明是谁的请求

            GetPost typ = GetPost.Get;

            public DataRetCallBackSucceed<T> onSucceed;
            public DataRetCallBackFailure onFailure;

            public void GetJsonData(string url, string tag = null, GetPost typ = GetPost.Get, IDictionary<string, string> parameters = null)
            {
                if (url == null)
                {
                    return;
                }

                this.url = url;
                this.parameters = parameters;
                this.typ = typ;
                this.tag = tag;

                reciveFunc();
                //            recive = new Thread(reciveFunc);
                //            recive.Start();
            }

            private void reciveFunc()
            {
                if (this.url == null)
                {
                    return;
                }

                HttpWebResponse hr = null;

                switch (typ)
                {
                    case GetPost.Get:
                        hr = HttpHelper.getInstance().CreateGetHttpResponse(url, 100, null, null);
                        break;
                    case GetPost.Post:
                        if (parameters != null)
                        {
                            hr = HttpHelper.getInstance().CreatePostHttpResponse(url, parameters, 100, null, null);
                        }
                        break;
                    default:
                        break;
                }

                if (hr == null)
                {
                    return;
                }

                string retjson = HttpHelper.getInstance().GetResponseString(hr);

                Console.WriteLine("\n" + retjson);

                // 检测返回结果
                BackDataType.MessageRetHead msghead = null;
                bool ret = false;
                do
                {
                    if (retjson == null)
                    {
                        break;
                    }

                    msghead = JsonDataHelp.getInstance().JsonDeserialize<BackDataType.MessageRetHead>(retjson);

                    if (Convert.ToInt64(msghead.code) > 0)
                    {
                        ret = false;
                    }
                    else
                    {
                        ret = true;
                    }

                } while (false);

                if (!ret)
                {
                    if (onFailure != null)
                    {
                        onFailure.Invoke(msghead, tag);
                    }

                    return;
                }

                T retdata = JsonDataHelp.getInstance().JsonDeserialize<T>(retjson);

                if (onSucceed != null)
                {
                    onSucceed.Invoke(retdata, retjson, tag, null);
                }
            }
        }
    }
}