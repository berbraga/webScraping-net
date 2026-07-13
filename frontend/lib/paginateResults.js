export const PAGE_SIZE = 60;

export function shouldShowPagination(total) {
  return Number(total) > PAGE_SIZE;
}

export function clampPage(page, total, pageSize = PAGE_SIZE) {
  const size = Math.max(1, Number(pageSize) || PAGE_SIZE);
  const count = Math.max(0, Number(total) || 0);
  const lastPage = Math.max(1, Math.ceil(count / size) || 1);
  const raw = Number(page);
  if (!Number.isFinite(raw) || raw < 1) {
    return 1;
  }
  return Math.min(Math.floor(raw), lastPage);
}

export function slicePage(items, page, pageSize = PAGE_SIZE) {
  const list = Array.isArray(items) ? items : [];
  const size = Math.max(1, Number(pageSize) || PAGE_SIZE);
  const safePage = clampPage(page, list.length, size);
  const start = (safePage - 1) * size;
  return list.slice(start, start + size);
}

export function pageRange(page, pageSize, total) {
  const size = Math.max(1, Number(pageSize) || PAGE_SIZE);
  const count = Math.max(0, Number(total) || 0);
  if (count === 0) {
    return { start: 0, end: 0 };
  }
  const safePage = clampPage(page, count, size);
  const start = (safePage - 1) * size + 1;
  const end = Math.min(safePage * size, count);
  return { start, end };
}

export function formatResultsFooter({
  total,
  page,
  pageSize = PAGE_SIZE,
  paginationActive,
  listTotal,
}) {
  const setTotal = Math.max(0, Number(total) || 0);
  const outerTotal = listTotal == null ? setTotal : Math.max(0, Number(listTotal) || 0);

  if (paginationActive) {
    const { start, end } = pageRange(page, pageSize, setTotal);
    return `Mostrando ${start}–${end} de ${setTotal}`;
  }

  return `${setTotal} de ${outerTotal} resultados`;
}
