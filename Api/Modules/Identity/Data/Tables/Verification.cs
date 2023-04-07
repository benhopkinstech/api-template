using System;
using System.Collections.Generic;

namespace Api.Modules.Identity.Data.Tables;

public partial class Verification
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Account Account { get; set; } = null!;
}
