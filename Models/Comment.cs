using System;
using System.Collections.Generic;

namespace ExcursionHelperDBEditor.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public string Commentator { get; set; } = null!;

    public string Commentary { get; set; } = null!;

    public int ExcursionId { get; set; }
    
    public DateTime CommentDate { get; set; }

    public virtual Excursion Excursion { get; set; } = null!;
}
