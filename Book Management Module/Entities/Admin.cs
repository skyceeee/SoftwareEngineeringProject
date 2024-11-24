using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Book_Management_Module.Entities
{
    public partial class Admin
    {
        public Admin()
        {
            staff = new HashSet<staff>();
        }

        public int AdminId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateTime HireDate { get; set; }

        [JsonIgnore]
        public virtual ICollection<staff> staff { get; set; }
    }
}
