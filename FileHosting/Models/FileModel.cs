using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FileHosting.Models
{
    public class FileModel
    {
        [Required]
        [Display(Name ="Title")]
        public string Title { get; set; }
        
        [Required]
        [Display(Name = "Text")]
        public string TextData { get; set; }
        [Required]
        [Display(Name ="File")]
        public IFormFile InputFile { get; set; }
        

        [Required]
        [Display(Name ="Is Deleted")]
        public bool IsDeleted { get; set; }
    }
}
