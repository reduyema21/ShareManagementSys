using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaccoShareManagementSys.Models
{
    public class ProfileImg
    {
        [Key]
        public int Id { get; set; }

        public string? FileName { get; set; }

        public byte[]? ImageData { get; set; }
        // to link the identity user
        public string UserId { get; set; } = string.Empty;
        [NotMapped]
        public IFormFile? UploadImage { get; set; }
    }
}