import { RouterProvider } from 'react-router-dom';
import { router } from './router/Router';
import { DemoModeBadge } from './components/ui/DemoModeBadge/DemoModeBadge';
import './App.css';

function App() {
  return (
    <>
      <DemoModeBadge />
      <RouterProvider router={router} />
    </>
  );
}

export default App;
