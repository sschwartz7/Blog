using Blog.Enums;
using Blog.Services.Interfaces;

namespace Blog.Services
{
    public class ImageService : IImageService
    {
        private readonly string? _defaultBlogImage = "/img/DefaultBlogImage.svg";
        private readonly string? _defaultUserImage = "/img/DefaultBlogUserImage.png";
        private readonly string? _defaultCategoryImage = "/img/DefaultCategoryImage.png";
        private readonly string? _blogAuthorImage = "/img/undraw_programing_re_kg9v.svg";
        public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, DefaultImage defaultImage)
        {
            try
            {
                if (fileData == null || fileData.Length == 0)
                {
                   switch(defaultImage)
                    {
                        case DefaultImage.AuthorImage: return _blogAuthorImage;
                        case DefaultImage.BlogPostImage: return _defaultBlogImage;
                        case DefaultImage.BlogUserImage: return _defaultUserImage;
                        case DefaultImage.CategoryImage: return _defaultCategoryImage;
                    }
                }
                string? imageBase64Data = Convert.ToBase64String(fileData!);
                imageBase64Data = string.Format($"data:{extension};base64,{imageBase64Data}");

                return imageBase64Data;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file)
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();//using cleans up after itself
                await file!.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();//actively closes memory stream
                return byteFile;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
