using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Common.Util
{
    public static class Generator
    {
        static IEnumerable<string> SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex.Split(input, @"(?<!^)(?=[A-Z])");
        }

        static IEnumerable<string> SplitVietnameseWords(string input)
        {
            var vietnameseMarks = new[] { 'à', 'á', 'ả', 'ã', 'ạ', 'â', 'ă', 'đ', 'è', 'é', 'ẻ', 'ẽ', 'ẹ', 'ê',
                                      'ì', 'í', 'ỉ', 'ĩ', 'ị', 'ò', 'ó', 'ỏ', 'õ', 'ọ', 'ô', 'ơ', 'ù', 'ú',
                                      'ủ', 'ũ', 'ụ', 'ư', 'ỳ', 'ý', 'ỷ', 'ỹ', 'ỵ' };

            var words = new List<string>();
            var currentWord = input[0].ToString();

            for (int i = 1; i < input.Length; i++)
            {
                if (vietnameseMarks.Contains(char.ToLower(input[i])) || char.IsUpper(input[i]))
                {
                    words.Add(currentWord);
                    currentWord = "";
                }
                currentWord += input[i];
            }

            if (!string.IsNullOrEmpty(currentWord))
            {
                words.Add(currentWord);
            }

            return words;
        }

        public static string GenerateTenantCode(Tenant tenant)
        {
            // Tách các từ dựa trên khoảng trắng, chữ hoa, và dấu
            var words = tenant.NameNonUnicode.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                  .SelectMany(SplitCamelCase)
                                  .SelectMany(SplitVietnameseWords);

            // Lấy chữ cái đầu tiên của mỗi từ
            string code = string.Join("", words.Select(w => w.First()));

            return $"{tenant.Code}_{code.ToUpper()}";
        }

        //public static string CodeTenentGenerator(string tenantName)
        //{
        //    // Split the input string into words based on whitespace
        //    var words = tenantName.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        //    // Extract the first letter of each word and concatenate them
        //    var name = string.Concat(words.Select(word => word[0]));

        //    return name.ToUpper(); // Convert the generated name to uppercase (optional)
        //}
        public static string UserNameGenerator(string userName, Tenant tenant)
        {
            return $"{tenant.Code}_{userName}";
        }
    }
}
