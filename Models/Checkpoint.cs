using System;
using System.Collections.Generic;

namespace ExcursionHelperDBEditor.Models;

public partial class Checkpoint
{
    public int CheckpointId { get; set; }

    public string Title { get; set; } = null!;

    public short? OrderNumber { get; set; }

    public virtual ICollection<Excursion> Excursions { get; set; } = new List<Excursion>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();
}
