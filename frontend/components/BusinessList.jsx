'use client';

function FieldValue({ value }) {
  if (value === null || value === undefined || value === '') {
    return <span className="missing" title="Indisponível">✕</span>;
  }
  return <span>{String(value)}</span>;
}

export default function BusinessList({ items, totalCount, emptyMessage }) {
  const rows = items || [];
  const total = totalCount ?? rows.length;

  if (rows.length === 0) {
    return (
      <div className="results-block">
        <p className="empty">{emptyMessage || 'Nenhum comércio encontrado.'}</p>
        {typeof totalCount === 'number' && (
          <p className="results-footer">0 de {total} resultados</p>
        )}
      </div>
    );
  }

  return (
    <div className="results-block">
      <div className="table-wrap">
        <table className="business-list">
          <thead>
            <tr>
              <th>Nome</th>
              <th>Telefone</th>
              <th>Site</th>
              <th>Avaliação</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((item) => (
              <tr key={item.id}>
                <td className="cell-name">{item.name}</td>
                <td><FieldValue value={item.phone} /></td>
                <td className="cell-site">
                  {item.website ? (
                    <a href={item.website} target="_blank" rel="noreferrer" title={item.website}>
                      {item.website}
                    </a>
                  ) : (
                    <FieldValue value={item.website} />
                  )}
                </td>
                <td><FieldValue value={item.rating} /></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="results-footer">{rows.length} de {total} resultados</p>
    </div>
  );
}
