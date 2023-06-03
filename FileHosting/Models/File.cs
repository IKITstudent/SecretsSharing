using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace FileHosting.Models
{
    public class File
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? TextData { get; set; }
        public string? FileName { get; set; }
        public string? Path { get; set; }
        public bool IsDelete { get; set; }
        [ForeignKey(nameof(User))]
        public string UserName { get; set; }
        public User Author { get; set; }

    }
}
