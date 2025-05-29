using Tasin.Website.Common.CommonModels;
using System.ComponentModel;

namespace Tasin.Website.Common.Helper
{
    public static class EnumHelper
    {
        /// <summary>
        /// Chuyển string sang enum kiểu T.
        /// Trả về true nếu thành công, false nếu thất bại.
        /// </summary>
        public static bool TryParseEnum<T>(string value, out T result, bool ignoreCase = true) where T : struct, Enum
        {
            return Enum.TryParse<T>(value, ignoreCase, out result);
        }

        /// <summary>
        /// Chuyển string sang enum kiểu T.
        /// Nếu thất bại sẽ ném lỗi.
        /// </summary>
        public static T ParseEnum<T>(string value, bool ignoreCase = true) where T : struct, Enum
        {
            if (Enum.TryParse<T>(value, ignoreCase, out var result))
            {
                return result;
            }
            throw new ArgumentException($"Không thể chuyển đổi '{value}' thành enum {typeof(T).Name}");
        }

        public static string GetEnumDescription(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
        }
        public static string GetEnumDescriptionByEnum(Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
        }

        public static List<KendoDropdownListModel<string>> ToDropdownList<TEnum>()
        {
            var enumValues = Enum.GetValues(typeof(TEnum));
            var result = new List<KendoDropdownListModel<string>>();
            foreach (Enum e in enumValues)
            {
                var description = e.GetEnumDescription();
                result.Add(new KendoDropdownListModel<string>
                {
                    Text = description,
                    Value = (Convert.ToInt32(e)).ToString(),
                    DataRaw = e.ToString()
                });
            }
            return result;
        }
        public static List<KendoDropdownListModel<string>> ToDropdownListStr<TEnum>()
        {
            var enumValues = Enum.GetValues(typeof(TEnum));
            var result = new List<KendoDropdownListModel<string>>();
            foreach (Enum e in enumValues)
            {
                var description = e.GetEnumDescription();
                result.Add(new KendoDropdownListModel<string>
                {
                    Text = description,
                    Value = e.ToString(),
                    //Data = e.ToString()
                });
            }
            return result;
        }

    }
}
