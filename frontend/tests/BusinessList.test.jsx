import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import BusinessList from '../components/BusinessList';

describe('BusinessList', () => {
  it('renders business names', () => {
    render(
      <BusinessList
        items={[
          { id: '1', name: 'Padaria Central', phone: '11', website: null, rating: 4.5 },
        ]}
        totalCount={1}
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
      />,
    );
    expect(screen.getByText('1 de 8 resultados')).toBeInTheDocument();
  });
});
