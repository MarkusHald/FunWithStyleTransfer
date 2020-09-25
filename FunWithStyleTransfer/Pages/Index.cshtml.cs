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
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace FunWithStyleTransfer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly List<string> _allowedFileExtensions = new List<string>(){".jpg", ".png"};
        private readonly string _imageFolderPath = "wwwroot/Images";

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


        public async Task<IActionResult> OnPostUploadAsync(List<IFormFile> files)
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
            using (var stream = System.IO.File.Create(FilePath))
            {
                await FileUpload.FormFile.CopyToAsync(stream);
            }

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
