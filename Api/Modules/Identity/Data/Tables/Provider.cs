using System;
using System.Collections.Generic;

namespace Api.Modules.Identity.Data.Tables;

public partial class Provider
{
    public short Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Account> Account { get; } = new List<Account>();
}
