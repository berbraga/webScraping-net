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
