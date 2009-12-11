using System;
using System.Drawing;
using System.Collections;
using System.Net;
using System.Web;
using System.Text;
using System.IO;
using System.Threading;

namespace FX.Utility
{
    public class Gravatar
    {
        public EventHandler AsyncImageRetrieved;

        public string Email = string.Empty;
        public int Size = 48;
        public Image Image = null;
        public int CacheDays = 6;
        public bool DisableCache = false;

        public Gravatar(string Email, int Size)
        {
            this.Email = Email;
            this.Size = Size;
        }

        public void GetImage()
        {
            if (this.Email == string.Empty) return;

            if (!this.DisableCache) this.Image = GetCacheImage(this.Email, this.Size);
            if (this.Image == null)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(GetGravatarUrl(this.Email, this.Size));
                    request.Timeout = 5000;
                    request.ReadWriteTimeout = 8000;

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    DateTime date = DateTime.Parse(response.Headers["Last-Modified"]).Date;
                    if (date > new DateTime(1988, 1, 1))
                    {
                        this.Image = Image.FromStream(response.GetResponseStream());
                        if (!this.DisableCache) SetCacheImage(this.Image, this.Email, this.Size);
                    }
                }
                catch { }

                // in case it failed, get from cache but ignore date
                if (this.Image == null && !this.DisableCache) this.Image = GetCacheImage(this.Email, this.Size, true);
            }

            if (this.AsyncImageRetrieved != null)
                AsyncImageRetrieved(this, EventArgs.Empty);
        }

        public void GetImageAsync()
        {
            Thread t = new Thread(new ThreadStart(this.GetImage));
            t.Start();
        }

        private string GetGravatarUrl(string email, int size)
        {
            System.Text.StringBuilder url = new System.Text.StringBuilder();

            url.Append("http://www.gravatar.com/avatar.php?");
            url.Append("gravatar_id=" + GetMd5Hash(email.ToLower()));
            url.Append("&rating=G");
            url.Append("&size=" + size);

            return url.ToString();
        }

        private string GetMd5Hash(string input)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(Encoding.ASCII.GetBytes(input));

            StringBuilder hash = new StringBuilder();
            for (int i = 0; i < result.Length; i++) hash.Append(result[i].ToString("x2"));

            return hash.ToString();
        }

        private void SetCacheImage(Image image, string email, int size)
        {
            try
            {
                string file = GetEmailCachePath(email, size);
                if (File.Exists(file)) File.Delete(email);

                image.Save(file);
            }
            catch { }
        }

        private Image GetCacheImage(string email, int size)
        {
            return GetCacheImage(email, size, false);
        }

        private Image GetCacheImage(string email, int size, bool ignoredate)
        {
            Image img = null;

            try
            {
                string file = GetEmailCachePath(email, size);
                if (File.Exists(file))
                {
                    FileInfo fileinfo = new FileInfo(file);
                    if (fileinfo.CreationTime > DateTime.Now.AddDays(CacheDays * -1))
                        img = Image.FromFile(file);
                }
            }
            catch { }

            return img;
        }

        private string GetEmailCachePath(string email, int size)
        {
            return Path.Combine(Path.GetTempPath(), string.Format("{0}-{1}.gravatar", email.Replace("@", "-").Replace(".", "-"), size));
        }
    }
}
