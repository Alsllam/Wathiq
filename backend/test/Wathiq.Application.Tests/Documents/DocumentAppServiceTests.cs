using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Wathiq.Documents.DocumentTypes;
using Wathiq.Documents.Documents;
using Wathiq.Documents.Holders;
using Wathiq.Shared.Files;
using Xunit;

namespace Wathiq.Documents;

/* The FR "create document -> it is stored" happy path through the real service stack:
 * DI, authorization (always-allow in tests), unit of work, EF, seed data. Only the
 * authenticated user is faked. Concrete class lives in EntityFrameworkCore.Tests. */
public abstract class DocumentAppServiceTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IDocumentAppService _documents;
    private readonly IHolderAppService _holders;
    private readonly IDocumentTypeAppService _types;
    private readonly ICurrentPrincipalAccessor _principalAccessor;

    protected DocumentAppServiceTests()
    {
        _documents = GetRequiredService<IDocumentAppService>();
        _holders = GetRequiredService<IHolderAppService>();
        _types = GetRequiredService<IDocumentTypeAppService>();
        _principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    // Change() swaps the ambient principal until disposed - the test-side stand-in for a bearer token.
    private IDisposable ActAs(Guid userId, string userName) =>
        _principalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, userName)
        ], "test")));

    private async Task<DocumentDto> CreateSampleDocumentAsync()
    {
        // First holders call materialises the self holder for the current user (FR-DOC-007).
        var holders = await _holders.GetListAsync();
        var self = holders.Items.Single(h => h.IsSelf);

        var passport = (await _types.GetListAsync()).Items.Single(t => t.Code == "PASSPORT");

        return await _documents.CreateAsync(new CreateDocumentDto
        {
            HolderId = self.Id,
            DocumentTypeId = passport.Id,
            Number = "P-102030",
            IssueDate = new DateOnly(2026, 3, 1),
            ExpiryDate = new DateOnly(2036, 3, 1),
            Notes = "جواز سفر"
        });
    }

    [Fact]
    public async Task Created_Document_Can_Be_Read_Back()
    {
        using (ActAs(Guid.NewGuid(), "amina"))
        {
            var created = await CreateSampleDocumentAsync();

            var loaded = await _documents.GetAsync(created.Id);
            loaded.Number.ShouldBe("P-102030");
            loaded.ExpiryDate.ShouldBe(new DateOnly(2036, 3, 1));
            loaded.Status.ShouldBe(DocumentStatus.Active);
            loaded.DaysUntilExpiry!.Value.ShouldBePositive();

            var list = await _documents.GetListAsync(new GetDocumentListInput());
            list.TotalCount.ShouldBe(1);
            list.Items.Single().Id.ShouldBe(created.Id);
        }
    }

    [Fact]
    public async Task Foreign_Documents_Do_Not_Exist_For_Other_Users()
    {
        Guid foreignId;
        using (ActAs(Guid.NewGuid(), "amina"))
        {
            foreignId = (await CreateSampleDocumentAsync()).Id;
        }

        using (ActAs(Guid.NewGuid(), "badr"))
        {
            (await _documents.GetListAsync(new GetDocumentListInput())).TotalCount.ShouldBe(0);
            // 404, not 403: the service must not confirm the id exists (api.md §3.4).
            await Should.ThrowAsync<EntityNotFoundException>(() => _documents.GetAsync(foreignId));
        }
    }

    [Fact]
    public async Task Attachment_Upload_Download_Delete_Round_Trip()
    {
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3, 4, 5 };   // fake PNG payload

        using (ActAs(Guid.NewGuid(), "amina"))
        {
            var doc = await CreateSampleDocumentAsync();

            var uploaded = await _documents.UploadAttachmentAsync(doc.Id,
                new RemoteStreamContent(new MemoryStream(bytes), "scan.png", "image/png"));
            uploaded.MimeType.ShouldBe("image/png");
            uploaded.SizeBytes.ShouldBe(bytes.Length);

            (await _documents.GetAsync(doc.Id)).Attachments.ShouldHaveSingleItem().Id.ShouldBe(uploaded.Id);

            // Download returns the exact bytes the store holds.
            var content = await _documents.GetAttachmentContentAsync(doc.Id, uploaded.Id);
            using var downloaded = new MemoryStream();
            await content.GetStream().CopyToAsync(downloaded);
            downloaded.ToArray().ShouldBe(bytes);

            // Grab the blob key before deletion so the post-commit file removal is verifiable.
            var blobKey = (await GetRequiredService<IRepository<Document, Guid>>()
                    .GetAsync(doc.Id)).Attachments.Single().BlobKey;

            await _documents.DeleteAttachmentAsync(doc.Id, uploaded.Id);

            (await _documents.GetAsync(doc.Id)).Attachments.ShouldBeEmpty();
            // OnCompleted ran after the UoW committed: the bytes are gone from the store too.
            var store = GetRequiredService<IFileStore>();
            (await Should.ThrowAsync<BusinessException>(() => store.GetAsync(DocumentConsts.AttachmentContainer, blobKey)))
                .Code.ShouldBe(WathiqSharedErrorCodes.FileNotFound);
        }
    }

    [Fact]
    public async Task Unsupported_File_Types_Are_Rejected_Before_Storage()
    {
        using (ActAs(Guid.NewGuid(), "amina"))
        {
            var doc = await CreateSampleDocumentAsync();

            var ex = await Should.ThrowAsync<BusinessException>(() => _documents.UploadAttachmentAsync(doc.Id,
                new RemoteStreamContent(new MemoryStream([1, 2, 3]), "notes.txt", "text/plain")));

            ex.Code.ShouldBe(DocumentsErrorCodes.UnsupportedFileType);
            (await _documents.GetAsync(doc.Id)).Attachments.ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task Expiry_Before_Issue_Is_Rejected_By_The_Service()
    {
        using (ActAs(Guid.NewGuid(), "amina"))
        {
            var holders = await _holders.GetListAsync();
            var passport = (await _types.GetListAsync()).Items.Single(t => t.Code == "PASSPORT");

            var ex = await Should.ThrowAsync<BusinessException>(() => _documents.CreateAsync(new CreateDocumentDto
            {
                HolderId = holders.Items.Single().Id,
                DocumentTypeId = passport.Id,
                IssueDate = new DateOnly(2030, 1, 1),
                ExpiryDate = new DateOnly(2020, 1, 1)
            }));

            ex.Code.ShouldBe(DocumentsErrorCodes.ExpiryBeforeIssue);
        }
    }
}
