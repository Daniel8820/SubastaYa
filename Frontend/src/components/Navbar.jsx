import { Link, useNavigate, useLocation } from 'react-router-dom';

const Navbar = () => {
    const navigate = useNavigate();
    const location = useLocation();

    // Ocultamos el Navbar en la pantalla de Login
    if (location.pathname === '/login' || location.pathname === '/' || location.pathname === '/registro') {
        return null;
    }

    const handleCerrarSesion = () => {
        localStorage.removeItem('token');
        navigate('/login');
    };

    return (
        <nav className="navbar navbar-expand-lg navbar-dark bg-primary mb-4 shadow-sm">
            <div className="container">
                <Link className="navbar-brand fw-bold" to="/catalogo">SubastaYa</Link>
                
                <button 
                    className="navbar-toggler" 
                    type="button" 
                    data-bs-toggle="collapse" 
                    data-bs-target="#navbarNav"
                >
                    <span className="navbar-toggler-icon"></span>
                </button>
                
                <div className="collapse navbar-collapse" id="navbarNav">
                    <ul className="navbar-nav me-auto">
                        <li className="nav-item">
                            <Link className="nav-link" to="/catalogo">Catálogo</Link>
                        </li>
                        <li className="nav-item">
                            <Link className="nav-link" to="/mis-actividades">Mis Actividades</Link>
                        </li>
                    </ul>
                    <div className="d-flex align-items-center gap-3">
                        <Link to="/crear-subasta" className="btn btn-warning btn-sm fw-bold">
                            <i className="bi bi-plus-circle me-1"></i> Publicar Subasta
                        </Link>
                        
                        <button 
                            onClick={handleCerrarSesion} 
                            className="btn btn-outline-light btn-sm d-flex align-items-center gap-2"
                        >
                            <i className="bi bi-box-arrow-right"></i> Cerrar Sesión
                        </button>
                    </div>
                </div>          
            </div>
        </nav>
    );
};

export default Navbar;