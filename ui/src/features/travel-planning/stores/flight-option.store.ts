import { create } from 'zustand';
import type { FlightOption } from '../domain/TravelPlan';

interface FlightSearchResults {
    artifactKey: string;
    departureFlightOptions: FlightOption[];
    returnFlightOptions: FlightOption[];
}

interface FlightOptionStore {
    flightOption: FlightOption | null;
    flightSearchResults: FlightSearchResults | null;
    setFlightOption: (flightOption: FlightOption) => void;
    setFlightSearchResults: (results: FlightSearchResults) => void;
    updateFlightOption: (updates: Partial<FlightOption>) => void;
    clearFlightOption: () => void;
    clearFlightSearchResults: () => void;
    hasFlightOption: () => boolean;
    hasFlightSearchResults: () => boolean;
}

export const useFlightOptionStore = create<FlightOptionStore>((set, get) => ({
    flightOption: null,
    flightSearchResults: null,

    setFlightOption: (flightOption: FlightOption) =>
        set({ flightOption }),

    setFlightSearchResults: (results: FlightSearchResults) =>
        set({ flightSearchResults: results }),

    updateFlightOption: (updates: Partial<FlightOption>) =>
        set((state) => ({
            flightOption: state.flightOption
                ? { ...state.flightOption, ...updates }
                : null
        })),

    clearFlightOption: () =>
        set({ flightOption: null }),

    clearFlightSearchResults: () =>
        set({ flightSearchResults: null }),

    hasFlightOption: () => {
        return get().flightOption !== null;
    },

    hasFlightSearchResults: () => {
        return get().flightSearchResults !== null;
    }
}));