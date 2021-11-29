using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Settings;

namespace WbooruPlugin.EHentai
{
    [Export(typeof(SettingBase))]
    public class EhentaiSetting : SettingBase
    {
        public uint DetailPagePreviewImagesCount { get; set; } = 20;
    }
}
