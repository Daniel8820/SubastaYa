import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import Catalogo from './pages/Catalogo';
import Detalle from './pages/Detalle';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="/login" element={<Login />} />
        <Route path="/catalogo" element={<Catalogo />} />
        
        {/* Agregamos la ruta dinámica con el parámetro :id */}
        <Route path="/subasta/:id" element={<Detalle />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;