﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace veClassRoom.Room
{
    /// <summary>
    /// 负责平台数据的转换
    /// </summary>
    class UserInfor
    {
        public bool islogin = false;
        public bool isleader = false;

        // 课程列表
        public ArrayList GetCourseList()
        {
            // 只为测试
            ArrayList ret = new ArrayList();
            ret.Add("TestCourse");

            return ret;
        }
    }
}