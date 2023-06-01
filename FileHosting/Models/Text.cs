using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace FileHosting.Models
{
    public class Text
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string TextData { get; set; }
        public string FileName { get; set; }
        
        //File
        public bool IsDelete { get; set; }

    }
}
