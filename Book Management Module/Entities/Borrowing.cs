using System;
using System.Collections.Generic;

namespace Book_Management_Module.Entities
{
    public partial class Borrowing
    {
        public int BorrowId { get; set; }
        public int MemberId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = null!;

        public virtual Book Book { get; set; } = null!;
        public virtual Member Member { get; set; } = null!;
    }
}
