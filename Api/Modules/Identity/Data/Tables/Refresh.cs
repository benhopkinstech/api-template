using System;
using System.Collections.Generic;

namespace Api.Modules.Identity.Data.Tables;

public partial class Refresh
{
    public Guid Id { get; set; }

    public string Secret { get; set; } = null!;

    public Guid AccountId { get; set; }

    public bool IsUsed { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ExpiresOn { get; set; }

    public virtual Account Account { get; set; } = null!;
}
