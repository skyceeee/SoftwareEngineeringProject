﻿using System;
using System.Collections.Generic;

namespace Software_Project_2024_API;

public partial class Borrowing
{
    public int BorrowId { get; set; }

    public int MemberId { get; set; }

    public int BookId { get; set; }

    public DateOnly BorrowDate { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Book Book { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
