﻿using UnityEngine;

namespace US
{
    class AssetLoader : IResourceLoader
    {
        public AssetLoader(string path, LoadedCallback finishCallback = null) : base(path, finishCallback, LoadType.ASSET) { }
        public override void Load()
        {
            base.Load();
            Finish();
        }

        public override void LoadAsyc()
        {
            base.LoadAsyc();
            Debug.Log("AssetDatabase没有异步加载！调用Load()");
            Load();
        }
    }
}
