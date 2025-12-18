import { create } from 'zustand';
import type { Exchange } from '../domain/Exchange';

interface ExchangesStore {
    exchanges: Exchange[];
    addExchange: (exchange: Exchange) => void;
    clear: () => void;
}

export const useExchangesStore = create<ExchangesStore>((set) => ({
    exchanges: [],

    addExchange: (exchange: Exchange) =>
        set((state) => {
            // Check if exchange with this ID already exists
            const existsIndex = state.exchanges.findIndex(e => e.id === exchange.id);

            if (existsIndex !== -1) {
                // Replace existing exchange
                const updatedExchanges = [...state.exchanges];
                updatedExchanges[existsIndex] = exchange;
                return { exchanges: updatedExchanges };
            }

            // Add new exchange if it doesn't exist
            return { exchanges: [...state.exchanges, exchange] };
        }),

    clear: () =>
        set({ exchanges: [] })
}));
