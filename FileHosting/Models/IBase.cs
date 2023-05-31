using System;
using System.ComponentModel.DataAnnotations;

namespace FileHosting.Models
{
    public interface IBase
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DateOfUpload { get; set; }
        public string Warning { get; set; }
    }
}
