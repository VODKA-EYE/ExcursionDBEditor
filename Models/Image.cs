using System;
using System.Collections.Generic;

namespace ExcursionHelperDBEditor.Models;

public partial class Image
{
    public int ImageId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? ImageDescription { get; set; }

    public virtual ICollection<Checkpoint> Checkpoints { get; set; } = new List<Checkpoint>();
}
