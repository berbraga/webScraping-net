export function hasRating(value) {
  if (value === null || value === undefined || value === '') {
    return false;
  }
  if (typeof value === 'string' && value.trim().toLowerCase() === 'n/a') {
    return false;
  }
  const n = typeof value === 'number' ? value : Number(value);
  return Number.isFinite(n);
}

export function toRatingNumber(value) {
  return typeof value === 'number' ? value : Number(value);
}

export function nextRatingSortDirection(current) {
  if (current === 'desc') {
    return 'asc';
  }
  if (current === 'asc') {
    return 'desc';
  }
  return 'desc';
}

/**
 * @param {Array} items
 * @param {'asc'|'desc'|null|undefined} direction
 */
export function sortByRating(items, direction) {
  const list = Array.isArray(items) ? [...items] : [];
  if (direction !== 'asc' && direction !== 'desc') {
    return list;
  }

  const withRating = [];
  const withoutRating = [];

  for (const item of list) {
    if (hasRating(item?.rating)) {
      withRating.push(item);
    } else {
      withoutRating.push(item);
    }
  }

  withRating.sort((a, b) => {
    const left = toRatingNumber(a.rating);
    const right = toRatingNumber(b.rating);
    return direction === 'desc' ? right - left : left - right;
  });

  return [...withRating, ...withoutRating];
}
