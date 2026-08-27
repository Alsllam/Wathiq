using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Wathiq.Documents.DocumentTypes;

namespace Wathiq.Documents.Data;

/// <summary>Seeds the document-type catalogue (FR-DOC-001). Runs from DbMigrator and from the test host.</summary>
public class DocumentTypesDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    // (code, ar, en, default validity in months)
    private static readonly (string Code, string Ar, string En, int? Months)[] Catalogue =
    {
        ("NATIONAL_ID",     "الهوية الوطنية",   "National ID",           120),
        ("PASSPORT",        "جواز السفر",       "Passport",              120),
        ("DRIVING_LICENCE", "رخصة القيادة",     "Driving licence",       120),
        ("VEHICLE_REG",     "استمارة المركبة",  "Vehicle registration",   36),
        ("INSURANCE",       "وثيقة التأمين",    "Insurance policy",       12),
        ("CONTRACT",        "عقد",              "Contract",               12),
        ("PERMIT",          "تصريح",            "Permit",                 12),
        ("OTHER",           "أخرى",             "Other",                 null),
    };

    private readonly IRepository<DocumentType, Guid> _documentTypes;
    private readonly IGuidGenerator _guidGenerator;

    public DocumentTypesDataSeedContributor(IRepository<DocumentType, Guid> documentTypes, IGuidGenerator guidGenerator)
    {
        _documentTypes = documentTypes;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Idempotent by Code: re-running the migrator (or the test host) must not duplicate rows.
        var existingCodes = (await _documentTypes.GetListAsync()).Select(t => t.Code).ToHashSet();

        var sortOrder = 0;
        foreach (var (code, ar, en, months) in Catalogue)
        {
            sortOrder += 10;
            if (existingCodes.Contains(code))
            {
                continue;
            }

            await _documentTypes.InsertAsync(new DocumentType(_guidGenerator.Create(), code, ar, en, months, sortOrder));
        }
    }
}
