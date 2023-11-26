using Microsoft.AspNetCore.Identity;
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
