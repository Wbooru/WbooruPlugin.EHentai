using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models;
using Wbooru.UI.ValueConverters;

namespace WbooruPlugin.EHentai.UI
{
    public class ImageInfo
    {
        public ImageSize PreviewImageSize { get; set; }
        public ImageAsyncLoadingParam ImageAsync { get; set; }
    }
}
