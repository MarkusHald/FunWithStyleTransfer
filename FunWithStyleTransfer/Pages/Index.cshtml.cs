using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Newtonsoft.Json;
using System.Buffers.Text;
using System.Text;

namespace FunWithStyleTransfer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly List<string> _allowedFileExtensions = new List<string>(){".jpg", ".png"};
        private readonly string _imageFolderPath = "wwwroot/Images";
        private static readonly HttpClient client = new HttpClient();

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        public Index FileUpload { get; set; }

        public string Result { get; private set; }

        public string FilePath { get; private set; }

        public string FileName { get; private set; }

        public void OnGet()
        {

        }


        public async Task<IActionResult> OnPostUploadAsync()
        {
            //Temporary solution - Delete all contents in images folder
            System.IO.DirectoryInfo di = new DirectoryInfo("wwwroot/Images");

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            // Find extension of filename and check that it conforms to either .jpg or .png
            var extension = Path.GetExtension(FileUpload.FormFile.FileName);
            if(!_allowedFileExtensions.Contains(extension))
            {
                Result = "Invalid file extension - Only .jpg and .png files are accepted";
                return Page();
            }

            //Create new filename
            FileName = Path.GetRandomFileName();            
            FileName = FileName.Replace(Path.GetExtension(FileName), extension);
            FilePath = Path.Combine(_imageFolderPath, FileName);

            //Load contents of iformfile into images folder
            string img;
            using (var stream = System.IO.File.Create(FilePath))
            {
                await FileUpload.FormFile.CopyToAsync(stream);
                
            }
            
            
            //Create base64 string representation of image and send to python web api for translation
            string s;
            using (var ms = new MemoryStream())
            {
                FileUpload.FormFile.CopyTo(ms);
                var fileBytes = ms.ToArray();
                s = Convert.ToBase64String(fileBytes);
            }
            string test = Convert.ToBase64String(System.IO.File.ReadAllBytes(FilePath));

            var values = new Dictionary<string, string>();
            values.Add("image", test);


            string json = JsonConvert.SerializeObject(values, Formatting.Indented);

            var content = new StringContent(json, Encoding.ASCII, "application/json");
            var response = await client.PostAsync("http://local-host:8/api/styleTransfer", content);
            var responseString = await response.Content.ReadAsStringAsync();


            return Page();
       }

    }


    public class Index
    {
        [Required]
        [Display(Name = "File")]
        public IFormFile FormFile { get; set; }

        [Display(Name = "Note")]
        [StringLength(50, MinimumLength = 0)]
        public string Note { get; set; }
    }
}
