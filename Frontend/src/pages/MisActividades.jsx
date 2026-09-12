import { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';

const MisActividades = () => {
    const navigate = useNavigate();
    const location = useLocation();

    const [actividades, setActividades] = useState({ misPublicaciones: [], misComprasYPujas: [] });
    const [cargando, setCargando] = useState(true);
    const [error, setError] = useState('');
    const [pestanaActiva, setPestanaActiva] = useState(location.state?.tab || 'publicaciones');
    
    // ESTADO PARA EL FILTRO EN MEMORIA
    const [filtroEstado, setFiltroEstado] = useState('');

    useEffect(() => {
        const obtenerActividades = async () => {
            const token = localStorage.getItem('token');
            if (!token) {
                navigate('/login');
                return;
            }

            try {
                const response = await fetch('https://localhost:7109/api/v1/users/me/activities', {
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                });

                if (response.ok) {
                    const data = await response.json();
                    setActividades(data);
                } else {
                    setError('Error al cargar las actividades.');
                }
            } catch (err) {
                setError('Error de conexión con el servidor.');
            } finally {
                setCargando(false);
            }
        };

        obtenerActividades();
    }, [navigate]);

    // LÓGICA DE FILTRADO EN MEMORIA (No toca el backend, es instantáneo)
    const publicacionesFiltradas = actividades.misPublicaciones.filter(pub => {
        if (filtroEstado === '') return true; // Si no hay filtro, mostramos todo
        return pub.estado === filtroEstado;
    });

    const pujasFiltradas = actividades.misComprasYPujas.filter(part => {
        if (filtroEstado === '') return true;
        return part.estado === filtroEstado;
    });

    if (cargando) return <div className="text-center mt-5"><h4>Cargando tus actividades...</h4></div>;
    if (error) return <div className="alert alert-danger mt-5 container">{error}</div>;

    return (
        <div className="container mt-4">
            <h2 className="mb-4 text-primary">Mis Actividades</h2>

            {/* Panel de Filtros Rápido */}
            <div className="bg-light p-3 rounded shadow-sm mb-4 d-flex align-items-center gap-3">
                <label className="fw-bold text-muted mb-0">Filtrar por estado:</label>
                <select 
                    className="form-select w-auto" 
                    value={filtroEstado} 
                    onChange={(e) => setFiltroEstado(e.target.value)}
                >
                    <option value="">Todos los estados</option>
                    <option value="ACTIVA">Activas</option>
                    <option value="PROGRAMADA">Programadas</option>
                    <option value="FINALIZADA">Finalizadas</option>
                    <option value="CANCELADA">Canceladas</option>
                    <option value="DESIERTA">Desiertas</option>
                </select>
            </div>

            {/* Pestañas de navegación */}
            <ul className="nav nav-tabs mb-4">
                <li className="nav-item">
                    <button 
                        className={`nav-link ${pestanaActiva === 'publicaciones' ? 'active fw-bold' : ''}`}
                        onClick={() => setPestanaActiva('publicaciones')}
                    >
                        Mis Publicaciones ({publicacionesFiltradas.length})
                    </button>
                </li>
                <li className="nav-item">
                    <button 
                        className={`nav-link ${pestanaActiva === 'pujas' ? 'active fw-bold' : ''}`}
                        onClick={() => setPestanaActiva('pujas')}
                    >
                        Mis Pujas y Compras ({pujasFiltradas.length})
                    </button>
                </li>
            </ul>

            {/* Contenido Dinámico */}
            <div className="row g-4">
                {pestanaActiva === 'publicaciones' && (
                    publicacionesFiltradas.length === 0 ? (
                        <div className="col-12">
                            <div className="alert alert-info">
                                {actividades.misPublicaciones.length === 0 
                                    ? "No tenés publicaciones." 
                                    : "No tenés publicaciones que coincidan con este filtro."}
                            </div>
                        </div>
                    ) : (
                        publicacionesFiltradas.map((pub) => (
                            <div key={pub.id} className="col-md-6 col-lg-4">
                                <div className={`card h-100 shadow-sm ${pub.adjudicada ? 'border-success' : ''}`}>
                                    <div className="card-body">
                                        <h5 className="card-title text-primary">{pub.titulo}</h5>
                                        <span className={`badge mb-3 ${pub.estado === 'ACTIVA' ? 'bg-success' : 'bg-secondary'}`}>
                                            {pub.estado}
                                        </span>
                                        <p className="card-text mb-1">
                                            Recaudación actual: <strong>${pub.recaudacion}</strong>
                                        </p>
                                        {pub.adjudicada && <span className="badge bg-success mt-2"><i className="bi bi-check-circle"></i> ¡Venta realizada!</span>}
                                    </div>
                                    <div className="card-footer bg-white border-top-0">
                                        <Link 
                                            to={`/subasta/${pub.id}`} 
                                            state={{ origen: '/mis-actividades', tab: 'publicaciones' }} 
                                            className="btn btn-outline-primary w-100"
                                        >
                                            Ver Detalle
                                        </Link>
                                    </div>
                                </div>
                            </div>
                        ))
                    )
                )}

                {pestanaActiva === 'pujas' && (
                    pujasFiltradas.length === 0 ? (
                        <div className="col-12">
                            <div className="alert alert-info">
                                {actividades.misComprasYPujas.length === 0 
                                    ? "Aún no participaste en ninguna subasta." 
                                    : "No tenés participaciones que coincidan con este filtro."}
                            </div>
                        </div>
                    ) : (
                        pujasFiltradas.map((part) => (
                            <div key={part.id} className="col-md-6 col-lg-4">
                                <div className={`card h-100 shadow-sm ${part.soyGanador ? 'border-warning' : ''}`}>
                                    <div className="card-body">
                                        <h5 className="card-title text-primary">{part.titulo}</h5>
                                        <span className={`badge mb-3 ${part.estado === 'ACTIVA' ? 'bg-success' : 'bg-secondary'}`}>
                                            {part.estado}
                                        </span>
                                        <div className="mb-2">
                                            <small className="text-muted d-block">Mi oferta máxima:</small>
                                            <span className="fs-5">${part.miOfertaMaxima}</span>
                                        </div>
                                        <div>
                                            <small className="text-muted d-block">Oferta ganadora actual:</small>
                                            <span className="fs-5">${part.ofertaGanadoraActual}</span>
                                        </div>
                                        {part.soyGanador && <span className="badge bg-warning text-dark mt-3"><i className="bi bi-trophy"></i> ¡Ganaste esta subasta!</span>}
                                    </div>
                                    <div className="card-footer bg-white border-top-0">
                                        <Link 
                                            to={`/subasta/${part.id}`} 
                                            state={{ origen: '/mis-actividades', tab: 'pujas' }} 
                                            className="btn btn-outline-primary w-100"
                                        >
                                            Ver Detalle
                                        </Link>
                                    </div>
                                </div>
                            </div>
                        ))
                    )
                )}
            </div>
        </div>
    );
};

export default MisActividades;