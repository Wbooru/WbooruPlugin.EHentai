using Castle.DynamicProxy.Generators;
using EHentaiAPI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Settings;
using Wbooru.Settings.UIAttributes;

namespace WbooruPlugin.EHentai
{
    [Export(typeof(SettingBase))]
    public class EhentaiSetting : SettingBase, ISharedPreferences
    {
        [Group("View Options")]
        [NameAlias("画廊预览图片列表数量")]
        public uint DetailPagePreviewImagesCount { get; set; } = 20;

        [Group("View Options")]
        [Description("这样会让你的账号流量流量花费更快(用完就会出现509错误)")]
        [NameAlias("浏览大图时优先加载原图")]
        public bool OriginImageRequire { get; set; } = false;

        [Ignore]
        public Dictionary<string, object> EhSettingMap { get; set; } = new();

        public T getValue<T>(string key, T defValue = default)
        {
            if (EhSettingMap.TryGetValue(key, out var d))
                return (T)d;
            return defValue;
        }

        public ISharedPreferences setValue<T>(string key, T value = default)
        {
            EhSettingMap[key] = value;
            return this;
        }
    }
}
