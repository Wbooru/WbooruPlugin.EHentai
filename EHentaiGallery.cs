using EHentaiAPI;
using EHentaiAPI.Client;
using EHentaiAPI.Client.Data;
using EHentaiAPI.ExtendFunction;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Wbooru;
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Models.Gallery;
using Wbooru.Settings;
using Wbooru.UI.Pages;
using WbooruPlugin.EHentai.UI;
using WbooruPlugin.EHentai.UI.Pages;
using WbooruPlugin.EHentai.Utils;

namespace WbooruPlugin.EHentai
{
    [Export(typeof(Gallery))]
    public class EHentaiGallery : Gallery, ICustomDetailImagePage, IGalleryAccount
    {
        private EhClient client;

        public override string GalleryName => "EHentai";

        public bool IsLoggined => client.Cookies.GetCookies(new System.Uri(client.EhUrl.GetHost()))?.Any(x => x.Name.Equals("ipb_pass_hash", System.StringComparison.InvariantCultureIgnoreCase) || x.Name.Equals("ipb_member_id", System.StringComparison.InvariantCultureIgnoreCase)) ?? false;

        public CustomLoginPage CustomLoginPage => new DefaultLoginPage(this);

        public EHentaiGallery()
        {
            client = new EhClient();
            Request.RequestFactory = (url) => new EHentaiRequest(url);
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

        public async Task AccountLoginAsync(AccountInfo info)
        {
            try
            {
                var respUserName = await client.SignInAsync(info.Name, info.Password);
                if (respUserName?.Equals(info.Name, System.StringComparison.InvariantCultureIgnoreCase) ?? false)
                {
                    Log<EHentaiGallery>.Info("login successfully.");
                }
            }
            catch (Exception e)
            {
                Log<EHentaiGallery>.Error(e.Message);
            }
        }

        public Task AccountLogoutAsync()
        {
            //nothing to do.
            return Task.CompletedTask;
        }
    }
}
