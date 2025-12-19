import { create } from 'zustand';
import type { Status } from '../domain/Status';

interface StatusStore {
    statuses: Status[];
    activeStatus: Status | null;
    addStatus: (status: Status) => void;
}

export const useStatusStore = create<StatusStore>((set) => ({
    statuses: [],
    activeStatus: null,
    addStatus: (status: Status) =>
        set((state) => ({
            statuses: [...state.statuses, status],
            activeStatus: status,
        })),
}));
