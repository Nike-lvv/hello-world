﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ko.NetFram
{
    public class SendObjectOnce : MonoBehaviour
    {
        private static SendObjectOnce _instance;
        public static SendObjectOnce getInstance()
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("SyncObjectOnce").GetComponent<SendObjectOnce>();
            }

            return _instance;
        }

        private Hashtable _allchange = new Hashtable();
        // 发送频率
        public float syncfreq = 0.01f;
        private float timer = 0;

        //// Use this for initialization
        //void Start()
        //{

        //}

        // Update is called once per frame
        void Update()
        {
            if (timer >= syncfreq)
            {
                timer = 0;
                if (_allchange.Count > 0)
                {
                    MsgModule.getInstance().req_all_change_once(_allchange);

                    foreach (DictionaryEntry de in _allchange)
                    {
                        Hashtable h = de.Value as Hashtable;
                        h.Clear();
                    }

                    _allchange.Clear();
                }
            }
            else
            {
                timer += Time.deltaTime;
            }
        }

        public void AddObject(string name, Hashtable change)
        {
            if (_allchange.ContainsKey(name))
            {
                _allchange[name] = change;
            }
            else
            {
                _allchange.Add(name, change);
            }
        }

        public void clearAll()
        {
            try
            {
                if (_allchange.Count > 0)
                {
                    foreach (DictionaryEntry de in _allchange)
                    {
                        Hashtable h = de.Value as Hashtable;
                        h.Clear();
                    }

                    _allchange.Clear();
                }
            }catch
            {

            }
        }

        private void OnDestroy()
        {
            clearAll();
        }
    }
}

