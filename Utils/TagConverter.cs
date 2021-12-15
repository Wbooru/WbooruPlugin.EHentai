using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models;

namespace WbooruPlugin.EHentai.Utils
{
    public static class TagConverter
    {
        public static TagType ConvertTagGroupToTagType(string ehTagGroupName) => ehTagGroupName.ToLower() switch
        {
            "artist" => TagType.Artist,

            "character" => TagType.Character,

            "male" => TagType.General,
            "female" => TagType.General,
            "mixed" => TagType.General,

            _ => TagType.Unknown
        };

        /// <summary>
        /// 将标签和标签类型转换成用于搜索用的关键词模式
        /// </summary>
        /// <param name="tagName">"futsu iyu"</param>
        /// <param name="ehTagGroupName">"artist"</param>
        /// <returns>artist:"futsu iyu$"</returns>
        public static string WrapTagForSearching(string tagName, string ehTagGroupName)
        {
            var wrapGroupName = ehTagGroupName.ToLower() switch
            {
                "artist" => "a",
                "character" => "c",
                "parody" => "parody",
                "male" => "m",
                "female" => "f",
                "mixed" => "x",
                "other" => "other",

                _ => string.Empty
            };

            var tag = $"{(tagName.Contains(" ") ? $"\"{tagName}$\"" : tagName)}";
            return $"{(string.IsNullOrWhiteSpace(wrapGroupName) ? string.Empty : $"{wrapGroupName}:")}{tag}";
        }
    }
}
