using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FileHosting.Models
{
    public class User : IdentityUser
    {
        public ObservableCollection<File> Files { get; set; } 
        public User() {
            Files = new ObservableCollection<File>();
        }
    }


}
