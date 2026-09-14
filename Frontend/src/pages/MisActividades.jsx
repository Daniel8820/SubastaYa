import { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import ImagenTarjeta from '../components/ImagenTarjeta';
import ContadorRegresivo from '../components/ContadorRegresivo';

const MisActividades = () => {
    const navigate = useNavigate();
    const location = useLocation();

    const [actividades, setActividades] = useState({ misPublicaciones: [], misComprasYPujas: [] });
    const [cargando, setCargando] = useState(true);
    const [error, setError] = useState('');
    const [pestanaActiva, setPestanaActiva] = useState(location.state?.tab || 'publicaciones');
    
    const [filtroEstado, setFiltroEstado] = useState('');

    useEffect(() => {
        const context = { isMounted: true };

        const timeoutId = setTimeout(() => {
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
                        if (context.isMounted) setActividades(data);
                    } else {
                        if (context.isMounted) setError('Error al cargar las actividades.');
                    }
                } catch (err) {
                    if (context.isMounted) setError('Error de conexión con el servidor.');
                } finally {
                    if (context.isMounted) setCargando(false);
                }
            };

            obtenerActividades();
        }, 250);

        return () => {
            context.isMounted = false;
            clearTimeout(timeoutId);
        };
    }, [navigate]);

    const publicacionesFiltradas = actividades.misPublicaciones.filter(pub => {
        if (filtroEstado === '') return true; // Si no hay filtro, mostramos todo
        return pub.estado === filtroEstado;
    });

    const pujasFiltradas = actividades.misComprasYPujas.filter(part => {
        if (filtroEstado === '') return true;
        return part.estado === filtroEstado;
    });

    if (cargando) return (
        <div className="text-center mt-5 py-5">
            <div className="spinner-border text-primary" role="status"></div>
            <h5 className="mt-3 text-muted">Cargando tus actividades...</h5>
        </div>
    );
    if (error) return <div className="alert alert-danger mt-5 container">{error}</div>;

    return (
        <div className="container mt-4">
            <h2 className="mb-4 text-dark">Mis Actividades</h2>

            {/* Panel de filtros rápido */}
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

            {/* Contenido dinámico */}
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
                                <div className={`card h-100 shadow-sm border-0 hover-animado ${pub.adjudicada ? 'border-success' : ''}`}>
                                    <ImagenTarjeta 
                                        src={pub.urlImagen} 
                                        alt={pub.titulo} 
                                        height="160px" 
                                    />
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
                                    <div className="card-footer bg-white border-top-0 pb-3">
                                        <div className="mb-3">
                                            <ContadorRegresivo 
                                                fechaInicio={pub.fechaInicio} 
                                                fechaFin={pub.fechaFin} 
                                                estado={pub.estado} 
                                            />
                                        </div>
                                        <Link 
                                            to={`/subasta/${pub.id}`} 
                                            state={{ origen: '/mis-actividades', tab: 'publicaciones' }} 
                                            className="btn btn-outline-primary w-100 fw-bold"
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
                        pujasFiltradas.map((part) => {
                           
                            const esActiva = part.estado === 'ACTIVA';
                            const soyLider = esActiva && part.miOfertaMaxima >= part.ofertaGanadoraActual;
                            const fuiSuperado = esActiva && part.miOfertaMaxima < part.ofertaGanadoraActual;

                            return (
                                <div key={part.id} className="col-md-6 col-lg-4">
                                    {/* Borde dinámico: Amarillo si ganó, neutral para el resto */}
                                    <div className={`card h-100 shadow-sm hover-animado ${part.soyGanador ? 'border-warning border-2' : 'border-0'}`}>
                                        <ImagenTarjeta 
                                            src={part.urlImagen} 
                                            alt={part.titulo} 
                                            height="160px" 
                                        />
                                        <div className="card-body">
                                            <h5 className="card-title text-primary mb-2">{part.titulo}</h5>
                                            
                                            {/* Fila de Badges de Estado y Liderazgo */}
                                            <div className="d-flex flex-wrap gap-2 mb-3">
                                                <span className={`badge ${esActiva ? 'bg-success' : 'bg-secondary'}`}>
                                                    {part.estado}
                                                </span>
                                                {soyLider && (
                                                    <span className="badge bg-primary shadow-sm"><i className="bi bi-star-fill me-1"></i>Vas ganando</span>
                                                )}
                                                {fuiSuperado && (
                                                    <span className="badge bg-danger shadow-sm"><i className="bi bi-exclamation-triangle-fill me-1"></i>Superado</span>
                                                )}
                                            </div>

                                            <div className="mb-2">
                                                <small className="text-muted d-block">Mi oferta máxima:</small>
                                                <span className={`fs-5 fw-bold ${fuiSuperado ? 'text-danger' : 'text-dark'}`}>
                                                    ${part.miOfertaMaxima}
                                                </span>
                                            </div>
                                            <div>
                                                <small className="text-muted d-block">Oferta ganadora actual:</small>
                                                <span className="fs-5 fw-bold text-success">${part.ofertaGanadoraActual}</span>
                                            </div>
                                            
                                            {part.soyGanador && (
                                                <div className="mt-3">
                                                    <span className="badge bg-warning text-dark"><i className="bi bi-trophy-fill me-1"></i>¡Ganaste esta subasta!</span>
                                                </div>
                                            )}
                                        </div>
                                        <div className="card-footer bg-white border-top-0 pb-3">
                                            <div className="mb-3">
                                                <ContadorRegresivo 
                                                    fechaInicio={part.fechaInicio} 
                                                    fechaFin={part.fechaFin} 
                                                    estado={part.estado} 
                                                />
                                            </div>
                                            <Link 
                                                to={`/subasta/${part.id}`} 
                                                state={{ origen: '/mis-actividades', tab: 'pujas' }} 
                                                // El botón se vuelve rojo si te pasaron
                                                className={`btn w-100 fw-bold ${fuiSuperado ? 'btn-danger' : 'btn-outline-primary'}`}
                                            >
                                                {fuiSuperado ? '¡Mejorar oferta!' : 'Ver Detalle'}
                                            </Link>
                                        </div>
                                    </div>
                                </div>
                            );
                        })
                    )
                )}
            </div>
        </div>
    );
};

export default MisActividades;