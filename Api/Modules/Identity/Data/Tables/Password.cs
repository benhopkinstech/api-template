using System;
using System.Collections.Generic;

namespace Api.Modules.Identity.Data.Tables;

public partial class Password
{
    public Guid AccountId { get; set; }

    public string Hash { get; set; } = null!;

    public DateTime? UpdatedOn { get; set; }

    public virtual Account Account { get; set; } = null!;
}
