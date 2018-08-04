using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UploadImage
{
    public class UploadAccount
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string NickName { get; set; }
        public string Mobile { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string AlbumId { get; set; }
        public string OpenKey { get; set; }
        public DateTime RegisterTime { get; set; }

    }
}
