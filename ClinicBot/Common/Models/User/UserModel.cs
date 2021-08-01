using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicBot.Common.Models.User
{
    public class UserModel
    {
        public string id { get; set; }
        public string userNameChannel { get; set; }
        public string channel { get; set; }
        public DateTime registerDate { get; set; }
    }
}
