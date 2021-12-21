using EHentaiAPI;
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
        const string ViewOptionsGroup = "画廊浏览";

        [Group(ViewOptionsGroup)]
        [NameAlias("画廊预览图片列表数量")]
        public uint DetailPagePreviewImagesCount { get; set; } = 20;

        [Group(ViewOptionsGroup)]
        [Description("这样会让你的账号流量流量花费更快(用完就会出现509错误)")]
        [NameAlias("浏览大图时优先加载原图")]
        public bool OriginImageRequire { get; set; } = false;

        [Group(ViewOptionsGroup)]
        [Description("以像素为单位")]
        [NameAlias("列表滚动速度")]
        public int ScrollOffsetSpeed { get; set; } = 30;

        [Group(ViewOptionsGroup)]
        [Description("当你浏览大图时，后台自动向后预加载页面的数量")]
        [NameAlias("浏览页面预加载后面页数")]
        public int ViewPageFrontPreload
        {
            get
            {
                return EhSettingMap.TryGetValue(Settings.KEY_SPIDER_FRONT_PRELOAD_COUNT, out var d) ? int.Parse(d.ToString()) : Settings.DEFAULT_SPIDER_FRONT_PRELOAD_COUNT;
            }
            set
            {
                EhSettingMap[Settings.KEY_SPIDER_FRONT_PRELOAD_COUNT] = value;
            }
        }

        [Group(ViewOptionsGroup)]
        [Description("当你浏览大图时，后台自动向前预加载页面的数量")]
        [NameAlias("浏览页面预加载前面页数")]
        public int ViewPageBackPreload
        {
            get
            {
                return EhSettingMap.TryGetValue(Settings.KEY_SPIDER_BACK_PRELOAD_COUNT, out var d) ? int.Parse(d.ToString()) : Settings.DEFAULT_SPIDER_BACK_PRELOAD_COUNT;
            }
            set
            {
                EhSettingMap[Settings.KEY_SPIDER_BACK_PRELOAD_COUNT] = value;
            }
        }

        #region ISharedPreferences

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

        #endregion
    }
}
