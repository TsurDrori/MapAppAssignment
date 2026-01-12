import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MapProvider } from '@/context/MapContext';
import { ToastProvider } from '@/components/ui/Toast';
import App from './App';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60, // 1 minute
      retry: 1,
    },
  },
});

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <MapProvider>
          <App />
        </MapProvider>
      </ToastProvider>
    </QueryClientProvider>
  </StrictMode>
);
