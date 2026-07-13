import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import BusinessList from '../components/BusinessList';

describe('BusinessList missing fields', () => {
  it('shows unavailable marker for null phone/website/rating', () => {
    render(
      <BusinessList
        items={[
          { id: '1', name: 'Loja Sem Contato', phone: null, website: null, rating: null },
        ]}
        totalCount={1}
      />,
    );

    const markers = screen.getAllByTitle('Indisponível');
    expect(markers).toHaveLength(4);
  });
});
