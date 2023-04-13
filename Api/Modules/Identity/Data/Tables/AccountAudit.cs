using System;
using System.Collections.Generic;

namespace Api.Modules.Identity.Data.Tables;

public partial class AccountAudit
{
    public long Id { get; set; }

    public Guid AccountId { get; set; }

    public string Email { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public virtual Account Account { get; set; } = null!;
}
