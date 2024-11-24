using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace Book_Management_Module.Entities
{
    public partial class Book
    {
        public Book()
        {
            Borrowings = new HashSet<Borrowing>();
        }

        public int BookId { get; set; }
        public string? BookName { get; set; }
        public string? Author { get; set; }
        public string? Genre { get; set; }
        [JsonIgnore]
        public virtual ICollection<Borrowing> Borrowings { get; set; }
    }
}
