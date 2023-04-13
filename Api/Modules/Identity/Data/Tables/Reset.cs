using System;
using System.Collections.Generic;
using System.Net;

namespace Api.Modules.Identity.Data.Tables;

public partial class Reset
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public IPAddress? CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Account Account { get; set; } = null!;
}
