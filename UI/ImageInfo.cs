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
    }

    public class ImageInfo<T> : ImageInfo
    {
        public T Param { get; set; }
    }

    public class DetailImageInfo : ImageInfo<(int ReferencePageIndex, string url)>, IVirtualGridFlowPanelItemParam
    {
        public double AspectRatio
        {
            get
            {
                return 1.0 * PreviewImageSize.Width / PreviewImageSize.Height;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool __HasInserted { get; set; }
        public int __ItemIndex { get; set; }
    }
}
