﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace US
{
    public class ResourceManager : MonoSingleton<ResourceManager>
    {
        /// <summary>
        /// 资源
        /// </summary>
        private readonly Dictionary<Type, Dictionary<string, AbstractResourceLoader>> _loadersPool = new Dictionary<Type, Dictionary<string, AbstractResourceLoader>>();
        /// <summary>
        /// 要卸载的资源列表
        /// </summary>
        private List<AbstractResourceLoader> unusedLoaderPool = new List<AbstractResourceLoader>();

        /// <summary>
        /// 最后一次GC时间
        /// </summary>
        private float lastGcTime = -1;
        /// <summary>
        /// GC间隔
        /// </summary>
        private static int GCIntervalTime = 1;

        /// <summary>
        /// 获取引用次数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public int GetRefCount<T>(string url)
        {
            AbstractResourceLoader loader = GetResourceLoader<T>(url);
            if (loader != null)
                return loader.GetRefCount();
            return 0;
        }

        /// <summary>
        /// 获取Loader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public AbstractResourceLoader GetResourceLoader<T>(string url)
        {
            Type type = typeof(T);
            return GetResourceLoader(type, url);
        }
        /// <summary>
        /// 获取Loader
        /// </summary>
        /// <param name="type"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public AbstractResourceLoader GetResourceLoader(Type type, string url)
        {
            var loaders = GetTypeDict(type);
            AbstractResourceLoader loader;
            if (loaders.TryGetValue(url, out loader))
            {
                return loader;
            }
            return null;
        }

        /// <summary>
        /// 获取类型字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Dictionary<string, AbstractResourceLoader> GetTypeDict<T>()
        {
            Type type = typeof(T);
            return GetTypeDict(type);
        }
        /// <summary>
        /// 获取类型字典
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Dictionary<string, AbstractResourceLoader> GetTypeDict(Type type)
        {
            Dictionary<string, AbstractResourceLoader> dict;
            if (_loadersPool.TryGetValue(type, out dict))
                return dict;
            return new Dictionary<string, AbstractResourceLoader>();
        }
        /// <summary>
        /// 增加loader
        /// </summary>
        /// <param name="loader"></param>
        public void AddResourceLoader(AbstractResourceLoader loader)
        {
            Type type = loader.GetType();
            if (_loadersPool[type] == null)
                _loadersPool[type] = new Dictionary<string, AbstractResourceLoader>();
            _loadersPool[type].Add(loader.Url, loader);
        }
        /// <summary>
        /// 移除loader
        /// </summary>
        /// <param name="loader"></param>
        public void RemoveResourceLoader(AbstractResourceLoader loader)
        {
            Type type = loader.GetType();
            if (_loadersPool[type] == null)
                return;
            if(_loadersPool[type].TryGetValue(loader.Url, out loader))
                _loadersPool[type].Remove(loader.Url);
            return;
        }
        /// <summary>
        /// 检查GC
        /// </summary>
        public void CheckCollect() {
            if (lastGcTime.Equals(-1) || (Time.time - lastGcTime) >= GCIntervalTime)
            {
                GarbageCollect();
                lastGcTime = Time.time;
            }
        }
        /// <summary>
        /// GC
        /// </summary>
        public void GarbageCollect() {
            foreach (var loader in _loadersPool) {
                foreach (var item in loader.Value)
                {
                    if(!item.Value.IsUnused())
                        unusedLoaderPool.Add(item.Value);
                }
            }

            foreach (var loader in unusedLoaderPool)
            {
                loader.Dispose();
                RemoveResourceLoader(loader);
            }
            unusedLoaderPool.Clear(); 
        }
    }
}