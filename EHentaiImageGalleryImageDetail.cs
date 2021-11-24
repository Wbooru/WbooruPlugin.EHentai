using EHentaiAPI.Client;
using EHentaiAPI.Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models;
using Wbooru.Models.Gallery;

namespace WbooruPlugin.EHentai
{
    public class EHentaiImageGalleryImageDetail : GalleryImageDetail
    {
        public GalleryDetail Detail { get; }

        public EHentaiImageGalleryImageDetail(GalleryDetail detail)
        {
            Detail = detail;
        }
    }
}
