using EHentaiAPI;
using EHentaiAPI.Client;
using EHentaiAPI.Client.Data;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Models.Gallery;
using Wbooru.UI.Pages;
using WbooruPlugin.EHentai.UI;
using WbooruPlugin.EHentai.UI.Pages;
using WbooruPlugin.EHentai.Utils;

namespace WbooruPlugin.EHentai
{
    [Export(typeof(Gallery))]
    public class EHentaiGallery : Gallery, ICustomDetailImagePage
    {
        private EhClient client;

        public override string GalleryName => "EHentai";

        public EHentaiGallery()
        {
            client = new EhClient();
            EHentaiAPI.ExtendFunction.Request.RequestFactory = (url) => new EHentaiRequest(url);
            client.Settings.PutGallerySite(EhUrl.SITE_E);
            //强制钦定一下列表样式
            client.Cookies.Add(new System.Net.Cookie("sl", "dm_1", "/", "e-hentai.org"));
        }

        public override GalleryItem GetImage(string id)
        {
            if (!IdConverter.TryConvertBackEhentai(id, out var gid, out var token))
            {
                return default;
            }

            var url = client.EhUrl.GetGalleryDetailUrl(gid, token);
            var detail = client.GetGalleryDetailAsync(url).Result;

            return EHentaiImageGalleryInfo.Create(client.EhUrl, detail);
        }

        public override GalleryImageDetail GetImageDetial(GalleryItem item)
        {
            if ((item as EHentaiImageGalleryInfo)?.CachedGalleryDetail is GalleryImageDetail d)
                return d;

            if (!IdConverter.TryConvertBackEhentai(item.GalleryItemID, out var gid, out var token))
                return default;

            var url = client.EhUrl.GetGalleryDetailUrl(gid, token);
            var detail = Task.Run(async () => await client.GetGalleryDetailAsync(url)).Result;

            return new EHentaiImageGalleryImageDetail(detail);
        }

        public override IEnumerable<GalleryItem> GetMainPostedImages()
        {
            var page = 0;
            var urlBuilder = new ListUrlBuilder(client.EhUrl);

            while (true)
            {
                if (page != 0)
                    urlBuilder.PageIndex = page;

                var url = urlBuilder.Build();

                var result = client.GetGalleryListAsync(url).Result;
                foreach (var info in result.galleryInfoList)
                    yield return EHentaiImageGalleryInfo.Create(client.EhUrl, info);

                if (result.galleryInfoList.Count == 0)
                    break;

                page++;
            }
        }

        public DetailImagePageBase GenerateDetailImagePageObject()
        {
            return new EHentaiGalleryDetailPage(client);
        }
    }
}
