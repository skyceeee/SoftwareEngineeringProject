using System;
using System.Collections.Generic;

namespace Book_Management_Module.Entities
{
    public partial class staff
    {
        public int StaffId { get; set; }
        public string Name { get; set; } = null!;
        public string Position { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public int? AdminId { get; set; }

        public virtual Admin? Admin { get; set; }
    }
}
