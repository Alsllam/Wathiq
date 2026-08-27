using System;

namespace Wathiq.Documents.DocumentTypes;

/// <summary>
/// Read model for the catalogue (FR-DOC-001). Both names travel together: the client picks
/// ar/en at render time, so switching language never needs a new request.
/// </summary>
public class DocumentTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string NameAr { get; set; } = default!;
    public string NameEn { get; set; } = default!;
    public int? DefaultValidityMonths { get; set; }
    public int SortOrder { get; set; }
}
