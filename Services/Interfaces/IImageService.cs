using Blog.Enums;

namespace Blog.Services.Interfaces
{
    public interface IImageService//everything in an interface is public
    {
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file);
        public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, DefaultImage defaultImage);

    }
}
