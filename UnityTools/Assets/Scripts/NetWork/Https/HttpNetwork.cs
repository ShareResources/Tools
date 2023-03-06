using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityEngine.Networking;
using Utilities;

namespace NetWork
{
    public enum RequestType
    {
        GET,
        POST,
        POSTUPLOAD
    }
    
    /// <summary>
    /// HTTP网络请求工具类
    /// </summary>
    public class HttpNetwork : Singleton<HttpNetwork>
    {
        private string tokenKey;
        private string tokenValue;
        /// <summary>
        /// 无参数的事件委托
        /// </summary>
        public delegate void Callback();
        
        /// <summary>
        /// 单参数的泛型委托
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        public delegate void Callback<T>(T args);

        /// <summary>
        /// 回调委托字典 key为url地址 value为对应的回调
        /// </summary>
        private readonly Dictionary<string, Delegate> table = new Dictionary<string, Delegate>();

        /// <summary>
        /// 网络请求进度
        /// </summary>
        private float mProgress = 0;

        public float GetProgress()
        {
            return mProgress;
        }

        public void SendHttpRequest(string url, RequestType requestType, params string[] parameterData)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();

            for (int i = 0; i < parameterData.Length; i++)
            {
                if(i * 2 +1 >= parameterData.Length)
                    break;
                data[parameterData[i * 2]] = parameterData[i * 2 + 1];
            }

            switch (requestType)
            {
                case RequestType.GET:
                    GameManager.Instance.StartCoroutine(HttpGet(url));
                    break;
                case RequestType.POST:
                    GameManager.Instance.StartCoroutine(HttpPost(url, data));
                    break;
                case RequestType.POSTUPLOAD:
                    break;
            }
        }

        IEnumerator HttpGet(string url)
        {
            string mContent;
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.SetRequestHeader(tokenKey, tokenValue);
                yield return webRequest.SendWebRequest();
                if (!string.IsNullOrEmpty(webRequest.error))
                {
                    //请求失败
                    mContent = webRequest.error;
                }
                else
                {
                    mProgress = webRequest.downloadProgress;
                    mContent = webRequest.downloadHandler.text;
                }
            }
            Dispatch(url,mContent);
        }

        IEnumerator HttpPost(string url, Dictionary<string, string> data)
        {
            string mContent;
            
            WWWForm form = new WWWForm();

            foreach (var postData in data)
            {
                form.AddField(postData.Key,postData.Value);
            }
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url,form))
            {
                webRequest.SetRequestHeader(tokenKey, tokenValue);
                yield return webRequest.SendWebRequest();
                if (!string.IsNullOrEmpty(webRequest.error))
                {
                    //请求失败
                    mContent = webRequest.error;
                }
                else
                {
                    mProgress = webRequest.downloadProgress;
                    mContent = webRequest.downloadHandler.text;
                }
            }
            Dispatch(url,mContent);
            
        }
        
        IEnumerator HttpPostUpload(string url, byte[] bytes)
        {
            string mContent;
            
            WWWForm form = new WWWForm();

            form.AddBinaryData("upload",bytes);
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url,form))
            {
                webRequest.SetRequestHeader(tokenKey, tokenValue);
                yield return webRequest.SendWebRequest();
                if (!string.IsNullOrEmpty(webRequest.error))
                {
                    //请求失败
                    mContent = webRequest.error;
                }
                else
                {
                    mProgress = webRequest.downloadProgress;
                    mContent = webRequest.downloadHandler.text;
                }
            }
            Dispatch(url,mContent);
            
        }

        void Dispatch<T>(string url, T arg)
        {
            if (!table.ContainsKey(url))
               return;
            Delegate delega = table[url];
            if (delega != null)
            {
                var callback = delega as Callback<T>;
                if (callback != null)
                    callback(arg);
            }
        }

        /// <summary>
        /// 监听url请求对应的相应
        /// </summary>
        /// <param name="url"></param>
        /// <param name="listener"></param>
        /// <typeparam name="T"></typeparam>
        public void AddListener<T>(string url, Callback<T> listener)
        {
            table[url] = table.ContainsKey(url) ? Delegate.Combine(table[url], listener) : listener;
        }

        /// <summary>
        /// 移除相应url请求响应监听
        /// </summary>
        /// <param name="url"></param>
        /// <param name="listener"></param>
        /// <typeparam name="T"></typeparam>
        public void RemoveListener<T>(string url, Callback<T> listener)
        {
            if (table.ContainsKey(url))
            {
                Delegate delega = Delegate.Remove(table[url], listener);
                if (delega == null)
                    table.Remove(url);
                else
                    table[url] = delega;

            }
        }

        public void RemoveListeners(string url)
        {
            table.Remove(url);
        }
    }
}