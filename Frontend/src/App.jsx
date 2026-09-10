import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import Catalogo from './pages/Catalogo';
import Detalle from './pages/Detalle';
import Navbar from './components/Navbar';
import MisActividades from './pages/MisActividades';
import Registro from './pages/Registro';
import CrearSubasta from './pages/CrearSubasta';

function App() {
  return (
    <BrowserRouter>
      <Navbar /> 
      <Routes>
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="/login" element={<Login />} />
        <Route path="/registro" element={<Registro />} />
        <Route path="/catalogo" element={<Catalogo />} />
        <Route path="/subasta/:id" element={<Detalle />} />
        <Route path="/mis-actividades" element={<MisActividades />} />
        <Route path="/crear-subasta" element={<CrearSubasta />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;