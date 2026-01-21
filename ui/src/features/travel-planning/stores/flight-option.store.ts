import { create } from 'zustand';
import type { FlightOption } from '../domain/TravelPlan';

interface FlightOptionStore {
    flightOption: FlightOption | null;
    setFlightOption: (flightOption: FlightOption) => void;
    updateFlightOption: (updates: Partial<FlightOption>) => void;
    clearFlightOption: () => void;
    hasFlightOption: () => boolean;
}

export const useFlightOptionStore = create<FlightOptionStore>((set, get) => ({
    flightOption: null,

    setFlightOption: (flightOption: FlightOption) =>
        set({ flightOption }),

    updateFlightOption: (updates: Partial<FlightOption>) =>
        set((state) => ({
            flightOption: state.flightOption
                ? { ...state.flightOption, ...updates }
                : null
        })),

    clearFlightOption: () =>
        set({ flightOption: null }),

    hasFlightOption: () => {
        return get().flightOption !== null;
    }
}));