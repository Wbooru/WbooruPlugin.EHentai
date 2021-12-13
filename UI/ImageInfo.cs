using EHentaiAPI.Client.Data;
using MikiraSora.VirtualizingStaggeredPanel;
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
        public GalleryPreview Preview { get; set; }
    }

    public class DetailImageInfo : ImageInfo, IVirtualGridFlowPanelItemParam
    {
        public string Url => Preview.ImageUrl;
        public int PageIndex => Preview.Position;

        public int ReferencePagesIndex { get; set; }
        public double AspectRatio { get; set; }

        //DONT TOUCH IT ^_^
        public bool __HasInserted { get; set; }
        public int __ItemIndex { get; set; }

        public override string ToString() => $"PageIndex:{PageIndex} RefPage:{ReferencePagesIndex} __ItemIndex:{__ItemIndex}";
    }
}
