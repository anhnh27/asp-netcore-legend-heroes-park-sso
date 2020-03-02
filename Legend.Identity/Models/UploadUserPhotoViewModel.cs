using System;
using Microsoft.AspNetCore.Http;

namespace Legend.Identity.Models
{
    public class UploadUserPhotoViewModel
    {
        public Guid FileName { get; set; }
        public IFormFile File { get; set; }
    }
}
