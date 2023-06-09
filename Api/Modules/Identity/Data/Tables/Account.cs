﻿using System;
using System.Collections.Generic;

namespace Api.Modules.Identity.Data.Tables;

public partial class Account
{
    public Guid Id { get; set; }

    public short ProviderId { get; set; }

    public string Email { get; set; } = null!;

    public bool IsVerified { get; set; }

    public DateTime? VerifiedOn { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual ICollection<AccountAudit> AccountAudit { get; } = new List<AccountAudit>();

    public virtual ICollection<Login> Login { get; } = new List<Login>();

    public virtual Password? Password { get; set; }

    public virtual ICollection<PasswordAudit> PasswordAudit { get; } = new List<PasswordAudit>();

    public virtual Provider Provider { get; set; } = null!;

    public virtual ICollection<Refresh> Refresh { get; } = new List<Refresh>();

    public virtual Reset? Reset { get; set; }

    public virtual Verification? Verification { get; set; }
}
