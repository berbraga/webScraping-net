export function filterByName(items, query) {
  const list = Array.isArray(items) ? items : [];
  const term = (query || '').trim().toLowerCase();
  if (!term) {
    return list;
  }
  return list.filter((item) => String(item?.name || '').toLowerCase().includes(term));
}

export function isProcessingStatus(status) {
  return status === 'pending' || status === 'running';
}

export function isRatingSortAllowed(status) {
  return !isProcessingStatus(status) && Boolean(status);
}

/**
 * Results table/toolbar are shown only after processing ends.
 * @param {{ status?: string|null, loading?: boolean }} opts
 */
export function shouldShowResultsArea({ status, loading = false } = {}) {
  if (loading) {
    return false;
  }
  if (!status) {
    return false;
  }
  if (isProcessingStatus(status)) {
    return false;
  }
  return status === 'completed' || status === 'cancelled' || status === 'failed';
}

export function statusLabel(status) {
  if (isProcessingStatus(status)) {
    return 'processando';
  }
  return status || '';
}

export function statusTone(status) {
  if (isProcessingStatus(status)) {
    return 'processing';
  }
  if (status === 'completed') {
    return 'completed';
  }
  if (status === 'failed') {
    return 'failed';
  }
  return 'neutral';
}

export function progressRatio(processedCount, totalFound) {
  const processed = Number(processedCount) || 0;
  const total = Number(totalFound) || 0;
  if (total <= 0) {
    return 0;
  }
  return Math.min(1, Math.max(0, processed / total));
}
