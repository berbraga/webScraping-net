import { apiRequest, getApiBaseUrl } from './apiClient';

export function createSearch({ region, query, maxResults }) {
  return apiRequest('/api/searches', {
    method: 'POST',
    body: JSON.stringify({ region, query, maxResults: Number(maxResults) || undefined }),
  });
}

export function getSearch(id) {
  return apiRequest(`/api/searches/${id}`);
}

/** Product absolute max results; keep list load complete for client-side pagination. */
export const LIST_BUSINESSES_TAKE = 200;

export function listBusinesses(id, { take = LIST_BUSINESSES_TAKE } = {}) {
  const params = new URLSearchParams({ take: String(take) });
  return apiRequest(`/api/searches/${id}/businesses?${params}`);
}

export function cancelSearch(id) {
  return apiRequest(`/api/searches/${id}/cancel`, { method: 'POST' });
}

export function exportSearchCsv(id) {
  return `${getApiBaseUrl()}/api/searches/${id}/export`;
}
