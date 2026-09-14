import { Link, useNavigate, useLocation } from 'react-router-dom';
import logo from '../assets/logo.png';

const Navbar = () => {
    const navigate = useNavigate();
    const location = useLocation();

    // Ocultamos el Navbar en las pantallas de ingreso
    if (location.pathname === '/login' || location.pathname === '/' || location.pathname === '/registro') {
        return null;
    }

    const handleCerrarSesion = () => {
        localStorage.removeItem('token');
        navigate('/login');
    };

    return (
        <>
            {/* NavBar superior */}
            <nav className="navbar navbar-expand-lg navbar-dark sticky-top mb-4 shadow-sm py-3" style={{ background: 'linear-gradient(90deg, #0d6efd 0%, #0099ff 100%)' }}>
                <div className="container">
                    
                    {/* Logo y marca más grandes */}
                    <Link className="navbar-brand fw-bold d-flex align-items-center gap-3 fs-4" to="/catalogo">
                        <img src={logo} alt="Logo SubastaYa" style={{ height: '42px', width: 'auto', objectFit: 'contain' }} />
                        SubastaYa
                    </Link>
                    
                    <button 
                        className="navbar-toggler" 
                        type="button" 
                        data-bs-toggle="collapse" 
                        data-bs-target="#navbarNav"
                    >
                        <span className="navbar-toggler-icon"></span>
                    </button>
                    
                    <div className="collapse navbar-collapse" id="navbarNav">
                        <ul className="navbar-nav me-auto gap-3 ms-lg-4">
                            <li className="nav-item">
                                <Link className="nav-link text-white fw-semibold d-flex align-items-center gap-2 fs-5 px-3 py-2" to="/catalogo">
                                    <i className="bi bi-grid-fill"></i> Catálogo
                                </Link>
                            </li>
                            <li className="nav-item">
                                <Link className="nav-link text-white fw-semibold d-flex align-items-center gap-2 fs-5 px-3 py-2" to="/mis-actividades">
                                    <i className="bi bi-list-task"></i> Mis Actividades
                                </Link>
                            </li>
                            <li className="nav-item">
                                <Link className="nav-link text-white fw-semibold d-flex align-items-center gap-2 fs-5 px-3 py-2" to="/billetera">
                                    <i className="bi bi-wallet2"></i> Billetera
                                </Link>
                            </li>
                        </ul>

                        {/* Menú desplegable de Usuario */}
                        <div className="dropdown">
                            <button 
                                className="btn btn-outline-light border-0 d-flex align-items-center gap-2 dropdown-toggle fs-5 px-3 py-2" 
                                type="button" 
                                id="userDropdown" 
                                data-bs-toggle="dropdown" 
                                aria-expanded="false"
                            >
                                <i className="bi bi-person-circle fs-4"></i> Mi Cuenta
                            </button>
                            <ul className="dropdown-menu dropdown-menu-end shadow border-0 mt-2 p-2" aria-labelledby="userDropdown">
                                <li>
                                    <Link className="dropdown-item d-flex align-items-center gap-2 py-2 rounded" to="/perfil">
                                        <i className="bi bi-person-gear text-primary fs-5"></i> Ajustes de Perfil
                                    </Link>
                                </li>
                                <li><hr className="dropdown-divider my-2" /></li>
                                <li>
                                    <button 
                                        onClick={handleCerrarSesion} 
                                        className="dropdown-item d-flex align-items-center gap-2 py-2 text-danger rounded"
                                    >
                                        <i className="bi bi-box-arrow-right fs-5"></i> Cerrar Sesión
                                    </button>
                                </li>
                            </ul>
                        </div>
                    </div>          
                </div>
            </nav>
        </>
    );
};

export default Navbar;