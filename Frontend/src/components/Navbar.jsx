import { useEffect, useRef } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import toast from 'react-hot-toast';
import logo from '../assets/logo.png';

const Navbar = () => {
    const navigate = useNavigate();
    const location = useLocation();
    
    // Referencia para guardar la conexión viva
    const connectionRef = useRef(null); 
    // Semáforo para saber si ya estamos en proceso de conexión
    const isConnectingRef = useRef(false); 

    useEffect(() => {
        const token = localStorage.getItem('token');
        
        // Si no hay token, ya estamos conectados o estamos ententando conectar, abortamos.
        if (!token || connectionRef.current || isConnectingRef.current) return;

        let miUsuarioId;
        try {
            miUsuarioId = parseInt(JSON.parse(atob(token.split('.')[1])).sub);
        } catch (e) { return; }

        //Sem en rojo
        isConnectingRef.current = true;

        const connection = new HubConnectionBuilder()
            .withUrl("https://localhost:7109/hubs/subasta")
            .configureLogging(LogLevel.Warning)
            .build();

        connection.start()
            .then(() => {
                connectionRef.current = connection;
                isConnectingRef.current = false; // Sem en verde

                connection.on("RecibirAlertaSuperacion", (data) => {
                    if (window.location.pathname !== `/subasta/${data.subastaId}`) {
                        toast(`¡Alguien superó tu oferta en "${data.titulo}"!`, {
                            icon: '⚠️',
                            style: { background: '#fff3cd', border: '1px solid #ffc107', color: '#856404' },
                            duration: 5000
                        });

                        const alertas = JSON.parse(localStorage.getItem('alertas_superacion') || '[]');
                        if (!alertas.includes(data.subastaId)) {
                            alertas.push(data.subastaId);
                            localStorage.setItem('alertas_superacion', JSON.stringify(alertas));
                            window.dispatchEvent(new Event('alertasActualizadas'));
                        }
                    }
                });
            })
            .catch(err => {
                console.error("Error Global SignalR:", err);
                isConnectingRef.current = false; // Liberamos el semáforo incluso si falla
            });
            
    }, [location.pathname]);

    // Ocultamos el Navbar en las pantallas de ingreso
    if (location.pathname === '/login' || location.pathname === '/' || location.pathname === '/registro') {
        return null;
    }

    const handleCerrarSesion = () => {
        localStorage.removeItem('token');
        if (connectionRef.current) {
            connectionRef.current.stop(); // Matamos el WebSocket al cerrar sesión
            connectionRef.current = null;
        }
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
                                <Link className="btn btn-outline-light border-0 fw-semibold d-flex align-items-center gap-2 fs-5 px-3 py-2" to="/catalogo">
                                    <i className="bi bi-grid-fill"></i> Catálogo
                                </Link>
                            </li>
                            <li className="nav-item">
                                <Link className="btn btn-outline-light border-0 fw-semibold d-flex align-items-center gap-2 fs-5 px-3 py-2" to="/mis-actividades">
                                    <i className="bi bi-list-task"></i> Mis Actividades
                                </Link>
                            </li>
                            <li className="nav-item">
                                <Link className="btn btn-outline-light border-0 fw-semibold d-flex align-items-center gap-2 fs-5 px-3 py-2" to="/billetera">
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