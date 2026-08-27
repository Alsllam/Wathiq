using System;
using System.ComponentModel.DataAnnotations;

namespace Wathiq.Documents.Holders;

public class HolderDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public HolderRelation Relation { get; set; }
    public DateOnly? BirthDate { get; set; }
    public bool IsSelf { get; set; }
}

/// <summary>
/// DataAnnotations here are the transport-level guard (ABP auto-validates DTOs and returns 400
/// before the service runs); the entity's Check.* guards stay the last line of defence.
/// </summary>
public class CreateHolderDto
{
    [Required]
    [StringLength(HolderConsts.MaxFullNameLength)]
    public string FullName { get; set; } = default!;

    public HolderRelation Relation { get; set; }

    public DateOnly? BirthDate { get; set; }
}

public class UpdateHolderDto
{
    [Required]
    [StringLength(HolderConsts.MaxFullNameLength)]
    public string FullName { get; set; } = default!;

    public DateOnly? BirthDate { get; set; }
}
