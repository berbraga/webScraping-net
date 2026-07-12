import './globals.css';

export const metadata = {
  title: 'Busca de Comércios no Google Maps',
  description: 'Coleta de comércios no Google Maps',
};

export default function RootLayout({ children }) {
  return (
    <html lang="pt-BR">
      <body>{children}</body>
    </html>
  );
}
