using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace Book_Management_Module.Entities
{
    public partial class Member
    {
        public Member()
        {
            Borrowings = new HashSet<Borrowing>();
        }

        public int MemberId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? JoinDate { get; set; }
        [JsonIgnore]
        public virtual ICollection<Borrowing> Borrowings { get; set; }
    }
}
