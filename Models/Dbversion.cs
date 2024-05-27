using System;
using System.Collections.Generic;

namespace ExcursionHelperDBEditor.Models;

public partial class Dbversion
{
    public int Version { get; set; }

    public DateTime Timestamp { get; set; }
}
