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

export function listBusinesses(id) {
  return apiRequest(`/api/searches/${id}/businesses`);
}

export function cancelSearch(id) {
  return apiRequest(`/api/searches/${id}/cancel`, { method: 'POST' });
}

export function exportSearchCsv(id) {
  return `${getApiBaseUrl()}/api/searches/${id}/export`;
}
