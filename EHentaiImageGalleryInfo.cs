using EHentaiAPI.Client;
using EHentaiAPI.Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;
using WbooruPlugin.EHentai.Utils;

namespace WbooruPlugin.EHentai
{
    public class EHentaiImageGalleryInfo : GalleryItem
    {
        public static GalleryItem Create(EhUrl ehUrl, GalleryInfo galleryInfo)
        {
            var item = new EHentaiImageGalleryInfo()
            {
                DetailLink = ehUrl.GetGalleryDetailUrl(galleryInfo),
                GalleryItemID = galleryInfo.ConvertToWbooruId(),
                GalleryName = galleryInfo.AvaliableTitle,
                PreviewImageSize = new Wbooru.Models.ImageSize(galleryInfo.ThumbWidth, galleryInfo.ThumbHeight),
                PreviewImageDownloadLink = galleryInfo.Thumb,
                DownloadFileName = $"{galleryInfo.ConvertToWbooruId()} {galleryInfo.AvaliableTitle}",
            };

            return item;
        }

        public static GalleryItem Create(EhUrl ehUrl, GalleryDetail detail)
        {
            var item = new EHentaiImageGalleryInfo()
            {
                DetailLink = ehUrl.GetGalleryDetailUrl(detail),
                GalleryItemID = detail.ConvertToWbooruId(),
                GalleryName = detail.AvaliableTitle,
                PreviewImageSize = new Wbooru.Models.ImageSize(detail.ThumbWidth, detail.ThumbHeight),
                PreviewImageDownloadLink = detail.Thumb,
                DownloadFileName = $"{detail.ConvertToWbooruId()} {detail.AvaliableTitle}",
            };

            item.CachedGalleryDetail = new EHentaiImageGalleryImageDetail(detail);

            return item;
        }

        public EHentaiImageGalleryImageDetail CachedGalleryDetail { get; set; }
    }
}
