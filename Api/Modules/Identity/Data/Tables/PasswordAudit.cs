using System;
using System.Collections.Generic;

namespace Api.Modules.Identity.Data.Tables;

public partial class PasswordAudit
{
    public long Id { get; set; }

    public Guid AccountId { get; set; }

    public string Hash { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public virtual Account Account { get; set; } = null!;
}
