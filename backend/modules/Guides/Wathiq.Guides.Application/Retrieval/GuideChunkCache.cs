using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Wathiq.Guides.Retrieval;

/// <summary>A chunk hydrated for scoring: bytes already decoded, entities left behind.</summary>
public record CachedChunk(Guid ChunkId, Guid GuideVersionId, int ChunkNo, string Text, float[] Vector, string Model);

/// <summary>
/// Scale honesty (5.4): SQL Server 2022 has no VECTOR type, and the corpus is a few hundred
/// chunks - so retrieval hydrates them ONCE (decoding varbinary → float[] on load, not per
/// question) and serves from memory under a TTL. SQL stays the source of truth; the embed job
/// invalidates on rebuild; the TTL covers writers this process can't see (a second host
/// instance). The growth path is named in the DB doc: VECTOR(1024) + native search when
/// SQL Server ships it - this class is the seam that swap replaces.
/// </summary>
public class GuideChunkCache : ISingletonDependency
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IReadOnlyList<CachedChunk>? _corpus;
    private DateTime _loadedAtUtc;

    public async Task<IReadOnlyList<CachedChunk>> GetOrLoadAsync(
        Func<Task<IReadOnlyList<CachedChunk>>> loader, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var snapshot = _corpus;
        if (snapshot is not null && DateTime.UtcNow - _loadedAtUtc < ttl)
        {
            return snapshot;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            // Double-check inside the gate: a concurrent question already reloaded for us.
            if (_corpus is not null && DateTime.UtcNow - _loadedAtUtc < ttl)
            {
                return _corpus;
            }

            _corpus = await loader();
            _loadedAtUtc = DateTime.UtcNow;
            return _corpus;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Called by the embed job after a rebuild - the next question sees fresh chunks immediately.</summary>
    public void Invalidate() => _corpus = null;
}
