import { useState, useEffect } from 'react';
import { useParams, Link, useLocation } from 'react-router-dom';
import { formatearFechaLocal } from '../utils/formatters';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import toast, { Toaster } from 'react-hot-toast';
import ContadorRegresivo from '../components/ContadorRegresivo';

const Detalle = () => {
    const { id } = useParams();
    const [subasta, setSubasta] = useState(null);
    const [cargando, setCargando] = useState(true);
    const [error, setError] = useState('');
    const [miUsuarioId, setMiUsuarioId] = useState(null);
    
    const [montoPuja, setMontoPuja] = useState('');
    const [enviando, setEnviando] = useState(false);
    const [cancelando, setCancelando] = useState(false); // Estado para el botón cancelar
    
    const location = useLocation();
    const rutaVolver = location.state?.origen || '/catalogo';
    const textoVolver = location.state?.origen === '/mis-actividades' ? 'Volver a Mis Actividades' : 'Volver al catálogo';
    const tabDeOrigen = location.state?.tab;

    // Función para anonimizar nombres
    const anonimizarNombre = (nombre) => {
        if (!nombre || nombre === 'Anónimo') return 'Anónimo';
        if (nombre.length <= 3) return nombre[0] + '***';
        return `${nombre.substring(0, 2)}***${nombre.substring(nombre.length - 2)}`;
    };

    const obtenerDetalle = async () => {
        try {
            const response = await fetch(`https://localhost:7109/api/v1/auctions/${id}`);
            if (response.ok) {
                const data = await response.json();
                setSubasta(data);
                
                const ofertaAlta = data.historialPujas.length > 0 ? data.historialPujas[0].monto : data.precioBase;
                const minimo = data.historialPujas.length > 0 ? ofertaAlta + data.incrementoMinimo : data.precioBase;
                setMontoPuja(minimo); 
            } else if (response.status === 404) {
                setError('La subasta solicitada no existe.');
            } else {
                setError('Error al cargar los datos.');
            }
        } catch (err) {
            setError('Error de conexión.');
        } finally {
            setCargando(false);
        }
    };

    useEffect(() => {
        const token = localStorage.getItem('token');
        if (token) {
            try {
                const payload = JSON.parse(atob(token.split('.')[1]));
                setMiUsuarioId(parseInt(payload.sub));
            } catch (e) {
                console.error("Token inválido");
            }
        }
        obtenerDetalle();
    }, [id]);

    useEffect(() => {
        const connection = new HubConnectionBuilder()
            .withUrl("https://localhost:7109/hubs/subasta")
            .configureLogging(LogLevel.Information)
            .build();

        connection.start()
            .then(() => {
                connection.invoke("UnirseASala", parseInt(id));

                connection.on("RecibirNuevaPuja", (nuevaPuja) => {
                    setSubasta((estadoAnterior) => {
                        if (!estadoAnterior) return estadoAnterior;

                        if (estadoAnterior.historialPujas.length > 0 && 
                            estadoAnterior.historialPujas[0].monto === nuevaPuja.monto) {
                            return estadoAnterior;
                        }

                        if (miUsuarioId && nuevaPuja.compradorId !== miUsuarioId) {
                            toast('¡Nueva oferta en la sala!', { icon: '🔥', style: { background: '#fff3cd' }});
                        }

                        const pujaFormateada = {
                            monto: nuevaPuja.monto,
                            comprador: nuevaPuja.comprador,
                            compradorId: nuevaPuja.compradorId,
                            fecha: nuevaPuja.fecha
                        };

                        return {
                            ...estadoAnterior,
                            pujasTotal: estadoAnterior.pujasTotal + 1,
                            historialPujas: [pujaFormateada, ...estadoAnterior.historialPujas]
                        };
                    });
                });
            })
            .catch(err => console.error("Error al conectar con SignalR:", err));
 
        return () => connection.stop();
    }, [id, miUsuarioId]);

    useEffect(() => {
        if (subasta) {
            const ofertaAlta = subasta.historialPujas.length > 0 
                ? subasta.historialPujas[0].monto 
                : subasta.precioBase;
            const minimoRequerido = ofertaAlta + subasta.incrementoMinimo;
            
            setMontoPuja((montoActual) => {
                if (!montoActual || parseFloat(montoActual) < minimoRequerido) {
                    return minimoRequerido;
                }
                return montoActual;
            });
        }
    }, [subasta]); 
    
    const handlePujar = async (e) => {
        e.preventDefault();
        setEnviando(true);

        const token = localStorage.getItem('token');
        if (!token) {
            toast.error('Debes iniciar sesión para pujar.');
            setEnviando(false);
            return;
        }

        try {
            const response = await fetch(`https://localhost:7109/api/v1/auctions/${id}/bids`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ monto: parseFloat(montoPuja) })
            });

            const data = await response.json();

            if (response.ok) {
                toast.success('¡Puja enviada con éxito!');
                obtenerDetalle(); 
            } else {
                toast.error(data.error || data.detail || 'Error al pujar.');
            }
        } catch (error) {
            toast.error('Error de conexión con el servidor.');
        } finally {
            setEnviando(false);
        }
    };

    // La función que realmente hace el fetch a la API (C#)
    const ejecutarCancelacion = async () => {
        setCancelando(true);
        const token = localStorage.getItem('token');

        try {
            const response = await fetch(`https://localhost:7109/api/v1/auctions/${id}/cancel`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                toast.success('Subasta cancelada exitosamente.');
                obtenerDetalle(); // Recarga la vista para que el estado pase a CANCELADA
            } else {
                const data = await response.json();
                toast.error(data.error || data.detail || 'Error al cancelar la subasta.');
            }
        } catch (error) {
            toast.error('Error de conexión con el servidor.');
        } finally {
            setCancelando(false);
        }
    };

    // El Toast interactivo
    const handleCancelarSubasta = () => {
        toast((t) => (
            <div>
                <p className="fw-bold mb-1 text-danger">
                    <i className="bi bi-exclamation-triangle-fill me-2"></i>
                    ¿Cancelar subasta?
                </p>
                <p className="small text-muted mb-3">
                    Esta acción no se puede deshacer y la subasta quedará inactiva permanentemente.
                </p>
                <div className="d-flex justify-content-end gap-2">
                    <button 
                        className="btn btn-sm btn-outline-secondary" 
                        onClick={() => toast.dismiss(t.id)}
                    >
                        Mantener activa
                    </button>
                    <button 
                        className="btn btn-sm btn-danger fw-bold" 
                        onClick={() => {
                            toast.dismiss(t.id); // Cerramos el toast
                            ejecutarCancelacion(); // Disparamos la API
                        }}
                    >
                        Sí, cancelar
                    </button>
                </div>
            </div>
        ), {
            duration: Infinity, // Infinity evita que el cartel se cierre solo por tiempo
            position: 'top-center',
            style: { border: '1px solid #dc3545', padding: '16px', maxWidth: '400px' }
        });
    };

    if (cargando) return <div className="text-center mt-5"><h4>Cargando detalle...</h4></div>;
    if (error) return <div className="alert alert-danger mt-5 container">{error}</div>;
    if (!subasta) return null;

    // Lógica de liderazgo
    const hayPujas = subasta.historialPujas.length > 0;
    const ofertaMasAlta = hayPujas ? subasta.historialPujas[0] : null;
    const soyLider = ofertaMasAlta?.compradorId === miUsuarioId;
    const participe = subasta.historialPujas.some(p => p.compradorId === miUsuarioId);
    const superado = participe && !soyLider;

    return (
        <div className="container mt-5">
            <Toaster position="top-right" reverseOrder={false} /> 
            
            <div className="mb-4">
                <Link to={rutaVolver} state={tabDeOrigen ? { tab: tabDeOrigen } : null} className="btn btn-secondary btn-sm">
                    &larr; {textoVolver}
                </Link>
            </div>

            <div className="row">
                <div className="col-md-8">
                    <div className="card shadow-sm mb-4">
                        <div className="card-body">
                            <div className="d-flex justify-content-between align-items-start mb-3">
                                <div>
                                    <h2 className="card-title text-primary mb-1">{subasta.titulo}</h2>
                                    <span className="text-muted"><i className="bi bi-tag-fill me-1"></i>{subasta.categoria || 'Sin categoría'}</span>
                                </div>
                                <div className="text-end">
                                    <span className={`badge fs-6 mb-2 ${subasta.estado === 'ACTIVA' ? 'bg-success' : 'bg-secondary'}`}>
                                        {subasta.estado}
                                    </span>
                                    <ContadorRegresivo fechaInicio={subasta.fechaInicio} fechaFin={subasta.fechaFin} estado={subasta.estado} enDetalle={true} />
                                </div>
                            </div>
                            <hr />
                            <p className="lead">{subasta.descripcion}</p>
                            
                            <div className="row text-center mt-4 bg-light p-3 rounded">
                                <div className="col-sm-4 border-end">
                                    <h6 className="text-muted mb-1">Precio Base</h6>
                                    <h5 className="mb-0">${subasta.precioBase}</h5>
                                </div>
                                <div className="col-sm-4 border-end">
                                    <h6 className="text-muted mb-1">Incremento Mínimo</h6>
                                    <h5 className="mb-0">${subasta.incrementoMinimo}</h5>
                                </div>
                                <div className="col-sm-4">
                                    <h6 className="text-muted mb-1">Vendedor</h6>
                                    <h5 className="mb-0">{anonimizarNombre(subasta.vendedor)}</h5>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="card shadow-sm">
                        <div className="card-header bg-white d-flex justify-content-between align-items-center py-3">
                            <h5 className="mb-0"><i className="bi bi-clock-history me-2"></i>Historial de Pujas en Vivo</h5>
                            <span className="badge bg-primary rounded-pill">{subasta.pujasTotal} ofertas</span>
                        </div>
                        <ul className="list-group list-group-flush">
                            {hayPujas ? (
                                subasta.historialPujas.map((puja, index) => (
                                    <li key={index} className={`list-group-item d-flex justify-content-between align-items-center ${index === 0 ? 'bg-light' : ''}`}>
                                        <div>
                                            <div className="d-flex align-items-center gap-2">
                                                <strong>{anonimizarNombre(puja.comprador)}</strong>
                                                {index === 0 && <span className="badge bg-warning text-dark" style={{fontSize: '0.7rem'}}>LÍDER</span>}
                                                {puja.compradorId === miUsuarioId && <span className="badge bg-info text-dark" style={{fontSize: '0.7rem'}}>TÚ</span>}
                                            </div>
                                            <div className="text-muted small mt-1"><i className="bi bi-calendar-event me-1"></i>{formatearFechaLocal(puja.fecha)}</div>
                                        </div>
                                        <span className={`fs-5 fw-bold ${index === 0 ? 'text-success' : 'text-muted'}`}>
                                            ${puja.monto}
                                        </span>
                                    </li>
                                ))
                            ) : (
                                <li className="list-group-item text-center text-muted py-5">
                                    <i className="bi bi-inbox fs-2 d-block mb-2"></i>
                                    Todavía no hay ofertas. ¡Sé el primero en pujar!
                                </li>
                            )}
                        </ul>
                    </div>
                </div>

                <div className="col-md-4 mt-4 mt-md-0">
                    <div className="card shadow-sm border-primary sticky-top" style={{top: '20px'}}>
                        <div className="card-body text-center">
                            <h4 className="text-primary mb-3">Consola de Ofertas</h4>
                            
                            <div className="bg-light rounded p-3 mb-4">
                                <span className="text-muted small text-uppercase fw-bold">Oferta Ganadora Actual</span>
                                <h2 className="text-success my-2">
                                    ${ofertaMasAlta ? ofertaMasAlta.monto : subasta.precioBase}
                                </h2>
                            </div>

                            {soyLider && (
                                <div className="alert alert-success fw-bold p-2 mb-3">
                                    <i className="bi bi-trophy-fill me-2"></i>¡Vas ganando la subasta!
                                </div>
                            )}
                            {superado && (
                                <div className="alert alert-danger fw-bold p-2 mb-3 animated pulse">
                                    <i className="bi bi-exclamation-triangle-fill me-2"></i>¡Fuiste superado!
                                </div>
                            )}

                            {subasta.vendedorId === miUsuarioId ? (
                                <>
                                    <div className="alert alert-warning text-start small mb-2">
                                        <i className="bi bi-info-circle me-2"></i>
                                        Esta es tu publicación. No podés pujar por tus propios artículos.
                                    </div>
                                    
                                    {/* Botón de Cancelación */}
                                    {subasta.historialPujas.length === 0 && (subasta.estado === 'ACTIVA' || subasta.estado === 'PROGRAMADA') ? (
                                        <button 
                                            onClick={handleCancelarSubasta}
                                            className="btn btn-outline-danger w-100 fw-bold shadow-sm mt-2"
                                            disabled={cancelando}
                                        >
                                            {cancelando ? (
                                                <><span className="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>Cancelando...</>
                                            ) : (
                                                <><i className="bi bi-x-circle me-2"></i>Cancelar Subasta</>
                                            )}
                                        </button>
                                    ) : (
                                        subasta.historialPujas.length > 0 && (subasta.estado === 'ACTIVA' || subasta.estado === 'PROGRAMADA') && (
                                            <div className="text-muted text-center small mt-3">
                                                <i className="bi bi-lock-fill me-1"></i>
                                                No podés cancelar esta subasta porque ya tiene ofertas registradas.
                                            </div>
                                        )
                                    )}
                                </>
                            ) : (
                                <form onSubmit={handlePujar}>
                                    <div className="form-group mb-3 text-start">
                                        <label className="form-label text-muted small fw-bold mb-1">Tu próxima oferta ($)</label>
                                        <div className="input-group input-group-lg">
                                            <span className="input-group-text bg-white"><i className="bi bi-currency-dollar"></i></span>
                                            <input 
                                                type="number" 
                                                className="form-control fw-bold text-primary" 
                                                value={montoPuja}
                                                onChange={(e) => setMontoPuja(e.target.value)}
                                                step="0.01"
                                                required
                                                disabled={enviando || subasta.estado !== 'ACTIVA' || soyLider}
                                            />
                                        </div>
                                        <div className="form-text text-center mt-2">
                                            Incremento mínimo: +${subasta.incrementoMinimo}
                                        </div>
                                    </div>
                                    
                                    <button 
                                        type="submit" 
                                        className="btn btn-primary btn-lg w-100 fw-bold shadow-sm" 
                                        disabled={enviando || subasta.estado !== 'ACTIVA' || soyLider}
                                    >
                                        {enviando ? (
                                            <><span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Enviando...</>
                                        ) : (
                                            'Enviar Puja'
                                        )}
                                    </button>
                                </form>
                            )}
                            
                            {subasta.estado !== 'ACTIVA' && subasta.estado !== 'PROGRAMADA' && (
                                <div className="text-danger fw-bold mt-3 border border-danger rounded p-2 bg-white">
                                    La subasta se encuentra {subasta.estado.toLowerCase()}.
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Detalle;