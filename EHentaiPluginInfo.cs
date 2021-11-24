using System;
using System.ComponentModel.Composition;
using Wbooru.PluginExt;

namespace WbooruPlugin.EHentai
{
    [Export(typeof(PluginInfo))]
    public class EHentaiPluginInfo : PluginInfo
    {
        public override string PluginName => "EHentai";

        public override string PluginProjectWebsite => "https://github.com/MikiraSora/WbooruPlugin.EHentai";

        public override string PluginAuthor => "DarkProjector";

        public override string PluginDescription => "Provide a gallery named EHentai.";
    }
}
