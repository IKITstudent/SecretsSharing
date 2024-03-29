﻿using System.ComponentModel.DataAnnotations;

namespace FileHosting.Models
{
    public class File
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? TextData { get; set; }
        public string? FileName { get; set; }
        public string Path { get; set; }
        public bool IsDelete { get; set; }
        public string UserName { get; set; }
        public User Author { get; set; }
        public int Views { get; set; }

    }
}
