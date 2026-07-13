import { fireEvent, render, screen, within } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
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

function ratingCells() {
  const table = screen.getByRole('table');
  const bodyRows = within(table).getAllByRole('row').slice(1);
  return bodyRows.map((row) => {
    const cells = within(row).getAllByRole('cell');
    return cells[3].textContent;
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
});
