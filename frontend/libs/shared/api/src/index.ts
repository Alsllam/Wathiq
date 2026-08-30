export * from './lib/provide-wathiq-api';

// The generated contract (npm run generate:api after any backend change). Components import
// the ALIASES below - the full .NET type names stay an implementation detail of the generator.
import type { components } from './lib/api-types.gen';

export type DocumentTypeDto = components['schemas']['Wathiq.Documents.DocumentTypes.DocumentTypeDto'];
export type DocumentDto = components['schemas']['Wathiq.Documents.Documents.DocumentDto'];
export type AttachmentDto = components['schemas']['Wathiq.Documents.Documents.AttachmentDto'];
export type HolderDto = components['schemas']['Wathiq.Documents.Holders.HolderDto'];
export type ExtractionProposalDto = components['schemas']['Wathiq.Documents.Extraction.ExtractionProposalDto'];

/// ABP's list envelopes, generic the way the wire actually is.
export interface ListResultDto<T> {
  items: T[];
}

export interface PagedResultDto<T> extends ListResultDto<T> {
  totalCount: number;
}
