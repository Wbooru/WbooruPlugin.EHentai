using EHentaiAPI.Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;

namespace WbooruPlugin.EHentai.Utils
{
    public static class IdConverter
    {
        public static string ConvertToWbooruId(long gid, string token) => $"{gid}_{token}";

        public static bool TryConvertBackEhentai(string wbooruId, out long gid, out string token)
        {
            gid = default;
            token = default;

            if (string.IsNullOrWhiteSpace(wbooruId))
                return false;

            var arr = wbooruId.Split("_");
            gid = int.Parse(arr[0]);
            token = arr[1];
            return true;
        }

        public static string ConvertToWbooruId(this GalleryInfo info) => ConvertToWbooruId(info.Gid, info.Token);
        internal static bool TryConvertBackEhentai(this GalleryItem info, out long gid, out string token) => TryConvertBackEhentai(info.GalleryItemID, out gid, out token);
    }
}
