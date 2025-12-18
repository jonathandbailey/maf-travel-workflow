import './App.css'
import RootLayout from './layout/RootLayout'
import streamingService from './api/streaming.api';
import { useEffect } from 'react';

function App() {

  useEffect(() => {
    const initializeStreaming = async () => {
      try {
        await streamingService.initialize();
        console.log('Streaming service initialized successfully');
      } catch (error) {
        console.error('Failed to initialize streaming service:', error);
      }
    };

    initializeStreaming();
  }, []);

  return (
    <>
      <RootLayout />
    </>
  )
}

export default App
