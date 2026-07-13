import { fireEvent, render, screen, within } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import BusinessList from '../components/BusinessList';

const ratedItems = [
  { id: '1', name: 'Low', phone: null, website: null, rating: 1 },
  { id: '2', name: 'High', phone: null, website: null, rating: 5 },
  { id: '3', name: 'Mid', phone: null, website: null, rating: 3 },
];

const mixedItems = [
  { id: '1', name: 'A', phone: null, website: null, rating: 4 },
  { id: '2', name: 'Missing', phone: null, website: null, rating: null },
  { id: '3', name: 'B', phone: null, website: null, rating: 2 },
];

function makeItems(count, namePrefix = 'Item') {
  return Array.from({ length: count }, (_, i) => ({
    id: String(i + 1),
    name: `${namePrefix} ${i + 1}`,
    phone: null,
    website: null,
    rating: (i % 5) + 1,
  }));
}

function ratingCells() {
  const table = screen.getByRole('table');
  const bodyRows = within(table).getAllByRole('row').slice(1);
  return bodyRows.map((row) => {
    const cells = within(row).getAllByRole('cell');
    return cells[4].textContent;
  });
}

function nameCells() {
  const table = screen.getByRole('table');
  const bodyRows = within(table).getAllByRole('row').slice(1);
  return bodyRows.map((row) => within(row).getAllByRole('cell')[0].textContent);
}

describe('BusinessList', () => {
  it('renders business names', () => {
    render(
      <BusinessList
        items={[
          { id: '1', name: 'Padaria Central', phone: '11', website: null, rating: 4.5 },
        ]}
        totalCount={1}
        searchStatus="completed"
      />,
    );

    expect(screen.getByText('Padaria Central')).toBeInTheDocument();
    expect(screen.getByText('1 de 1 resultados')).toBeInTheDocument();
  });

  it('renders empty state with footer count', () => {
    render(
      <BusinessList
        items={[]}
        totalCount={8}
        emptyMessage="Nenhum comércio encontrado."
        searchStatus="completed"
      />,
    );
    expect(screen.getByText('Nenhum comércio encontrado.')).toBeInTheDocument();
    expect(screen.getByText('0 de 8 resultados')).toBeInTheDocument();
  });

  it('shows filtered count in footer', () => {
    render(
      <BusinessList
        items={[{ id: '1', name: 'A', phone: null, website: null, rating: null }]}
        totalCount={8}
        searchStatus="completed"
      />,
    );
    expect(screen.getByText('1 de 8 resultados')).toBeInTheDocument();
  });

  it('first click sorts by rating descending when completed', () => {
    render(
      <BusinessList
        items={ratedItems}
        totalCount={3}
        searchStatus="completed"
        searchId="s1"
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: /ordenar por avaliação/i }));

    expect(ratingCells()).toEqual(['5', '3', '1']);
    expect(screen.getByText('↓')).toBeInTheDocument();
    expect(screen.getByRole('columnheader', { name: /avaliação/i })).toHaveAttribute(
      'aria-sort',
      'descending',
    );
  });

  it('ignores sort clicks while running', () => {
    render(
      <BusinessList
        items={ratedItems}
        totalCount={3}
        searchStatus="running"
        searchId="s1"
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: /ordenar por avaliação/i }));

    expect(nameCells()).toEqual(['Low', 'High', 'Mid']);
    expect(screen.queryByText('↓')).not.toBeInTheDocument();
  });

  it('toggles ascending then descending on subsequent clicks', () => {
    render(
      <BusinessList
        items={ratedItems}
        totalCount={3}
        searchStatus="completed"
        searchId="s1"
      />,
    );

    const button = screen.getByRole('button', { name: /ordenar por avaliação/i });
    fireEvent.click(button);
    fireEvent.click(button);
    expect(ratingCells()).toEqual(['1', '3', '5']);
    expect(screen.getByText('↑')).toBeInTheDocument();

    fireEvent.click(button);
    expect(ratingCells()).toEqual(['5', '3', '1']);
    expect(screen.getByText('↓')).toBeInTheDocument();
  });

  it('keeps missing ratings at the end after sort', () => {
    render(
      <BusinessList
        items={mixedItems}
        totalCount={3}
        searchStatus="completed"
        searchId="s1"
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: /ordenar por avaliação/i }));
    expect(nameCells()).toEqual(['A', 'B', 'Missing']);
  });

  it('resets sort when searchId changes', () => {
    const { rerender } = render(
      <BusinessList
        items={ratedItems}
        totalCount={3}
        searchStatus="completed"
        searchId="s1"
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: /ordenar por avaliação/i }));
    expect(ratingCells()).toEqual(['5', '3', '1']);

    rerender(
      <BusinessList
        items={ratedItems}
        totalCount={3}
        searchStatus="completed"
        searchId="s2"
      />,
    );

    expect(nameCells()).toEqual(['Low', 'High', 'Mid']);
    expect(screen.queryByText('↓')).not.toBeInTheDocument();
  });

  it('exposes sortable header affordance', () => {
    render(
      <BusinessList
        items={ratedItems}
        totalCount={3}
        searchStatus="completed"
      />,
    );
    const button = screen.getByRole('button', { name: /ordenar por avaliação/i });
    expect(button).toHaveClass('th-sort-button');
    expect(button.closest('th')).toHaveClass('th-sortable');
  });

  it('supports keyboard activation with Enter', () => {
    render(
      <BusinessList
        items={ratedItems}
        totalCount={3}
        searchStatus="completed"
        searchId="s1"
      />,
    );

    fireEvent.keyDown(screen.getByRole('button', { name: /ordenar por avaliação/i }), {
      key: 'Enter',
    });
    expect(ratingCells()).toEqual(['5', '3', '1']);
  });

  it('does not show pagination controls for 60 items and uses simple footer', () => {
    const items = makeItems(60);
    render(
      <BusinessList
        items={items}
        totalCount={60}
        searchStatus="completed"
        searchId="s1"
      />,
    );

    expect(screen.queryByRole('button', { name: /^próxima$/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /^anterior$/i })).not.toBeInTheDocument();
    expect(screen.getByText('60 de 60 resultados')).toBeInTheDocument();
    expect(nameCells()).toHaveLength(60);
  });

  it('paginates 125 items with next and disabled controls at ends', () => {
    const items = makeItems(125);
    render(
      <BusinessList
        items={items}
        totalCount={125}
        searchStatus="completed"
        searchId="s1"
      />,
    );

    expect(nameCells()).toHaveLength(60);
    expect(nameCells()[0]).toBe('Item 1');
    expect(screen.getByText('Mostrando 1–60 de 125')).toBeInTheDocument();

    const prev = screen.getByRole('button', { name: /^anterior$/i });
    const next = screen.getByRole('button', { name: /^próxima$/i });
    expect(prev).toBeDisabled();
    expect(next).toBeEnabled();

    fireEvent.click(next);
    expect(nameCells()[0]).toBe('Item 61');
    expect(screen.getByText('Mostrando 61–120 de 125')).toBeInTheDocument();

    fireEvent.click(next);
    expect(nameCells()).toHaveLength(5);
    expect(nameCells()[0]).toBe('Item 121');
    expect(screen.getByText('Mostrando 121–125 de 125')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /^próxima$/i })).toBeDisabled();
    expect(screen.getByRole('button', { name: /^anterior$/i })).toBeEnabled();
  });

  it('returns to page 1 with Anterior', () => {
    const items = makeItems(125);
    render(
      <BusinessList
        items={items}
        totalCount={125}
        searchStatus="completed"
        searchId="s1"
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: /^próxima$/i }));
    expect(nameCells()[0]).toBe('Item 61');

    fireEvent.click(screen.getByRole('button', { name: /^anterior$/i }));
    expect(nameCells()[0]).toBe('Item 1');
    expect(screen.getByRole('button', { name: /^anterior$/i })).toBeDisabled();
  });

  it('paginates only filtered matching names and hides controls at ≤60', () => {
    const many = makeItems(70, 'Cafe');
    const few = makeItems(25, 'Cafe');

    const { rerender } = render(
      <BusinessList
        items={many}
        totalCount={100}
        searchStatus="completed"
        searchId="s1"
      />,
    );

    expect(screen.getByRole('button', { name: /^próxima$/i })).toBeInTheDocument();
    expect(nameCells().every((name) => name.startsWith('Cafe'))).toBe(true);
    expect(screen.getByText('Mostrando 1–60 de 70')).toBeInTheDocument();

    rerender(
      <BusinessList
        items={few}
        totalCount={100}
        searchStatus="completed"
        searchId="s1"
      />,
    );

    expect(screen.queryByRole('button', { name: /^próxima$/i })).not.toBeInTheDocument();
    expect(nameCells()).toHaveLength(25);
    expect(screen.getByText('25 de 100 resultados')).toBeInTheDocument();
  });

  it('keeps global sort order across pages and resets page on sort or searchId', () => {
    const items = makeItems(65).map((item, index) => ({
      ...item,
      rating: 65 - index,
    }));

    const { rerender } = render(
      <BusinessList
        items={items}
        totalCount={65}
        searchStatus="completed"
        searchId="s1"
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: /ordenar por avaliação/i }));
    const page1Ratings = ratingCells();
    fireEvent.click(screen.getByRole('button', { name: /^próxima$/i }));
    const page2Ratings = ratingCells();
    const concatenated = [...page1Ratings, ...page2Ratings].map(Number);
    expect(concatenated).toEqual([...concatenated].sort((a, b) => b - a));

    fireEvent.click(screen.getByRole('button', { name: /ordenar por avaliação/i }));
    expect(screen.getByText(/Mostrando 1–60 de 65/)).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: /^próxima$/i }));
    rerender(
      <BusinessList
        items={items}
        totalCount={65}
        searchStatus="completed"
        searchId="s2"
      />,
    );
    expect(screen.getByText(/Mostrando 1–60 de 65/)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /^anterior$/i })).toBeDisabled();
  });

  it('paginates without invoking listBusinesses (client-only)', () => {
    const listBusinesses = vi.fn();
    const items = makeItems(125);
    render(
      <BusinessList
        items={items}
        totalCount={125}
        searchStatus="completed"
        searchId="s1"
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: /^próxima$/i }));
    fireEvent.click(screen.getByRole('button', { name: /^anterior$/i }));
    expect(listBusinesses).not.toHaveBeenCalled();
  });

  it('renders Criação do site column after Site with year or missing marker', () => {
    render(
      <BusinessList
        items={[
          {
            id: '1',
            name: 'Com Ano',
            phone: null,
            website: 'https://a.example',
            siteCreationYear: 2016,
            rating: null,
          },
          {
            id: '2',
            name: 'Sem Ano',
            phone: null,
            website: 'https://b.example',
            siteCreationYear: null,
            rating: null,
          },
        ]}
        totalCount={2}
        searchStatus="completed"
      />,
    );

    const headers = screen.getAllByRole('columnheader').map((h) => h.textContent);
    const siteIdx = headers.findIndex((h) => h === 'Site');
    const yearIdx = headers.findIndex((h) => h === 'Criação do site');
    expect(yearIdx).toBe(siteIdx + 1);
    expect(screen.getByText('2016')).toBeInTheDocument();
  });
});
