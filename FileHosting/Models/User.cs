using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;

namespace FileHosting.Models
{
    public class User : IdentityUser
    {
        public List<File> Files { get; set; }
    }


}
