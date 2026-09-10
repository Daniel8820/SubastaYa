import { useState, useEffect } from 'react';
import { useParams, Link, useLocation } from 'react-router-dom';
import { formatearFechaLocal } from '../utils/formatters';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

const Detalle = () => {
    const { id } = useParams();
    const [subasta, setSubasta] = useState(null);
    const [cargando, setCargando] = useState(true);
    const [error, setError] = useState('');
    const [miUsuarioId, setMiUsuarioId] = useState(null);
    
    // --- Estados para la Puja ---
    const [montoPuja, setMontoPuja] = useState('');
    const [mensajePuja, setMensajePuja] = useState({ tipo: '', texto: '' });
    const [enviando, setEnviando] = useState(false);
    const location = useLocation();
    const rutaVolver = location.state?.origen || '/catalogo';
    const textoVolver = location.state?.origen === '/mis-actividades' ? 'Volver a Mis Actividades' : 'Volver al catálogo';
    const tabDeOrigen = location.state?.tab; // Para devolverle la pestaña si es que vino de ahí

    // Extraemos la función fuera del useEffect para poder llamarla después de pujar
    const obtenerDetalle = async () => {
        try {
            const response = await fetch(`https://localhost:7109/api/v1/auctions/${id}`);
            if (response.ok) {
                const data = await response.json();
                setSubasta(data);
                
                // Calculamos el monto mínimo inicial para precargar el input
                const ofertaAlta = data.historialPujas.length > 0 ? data.historialPujas[0].monto : data.precioBase;
                const minimo = data.historialPujas.length > 0 ? ofertaAlta + data.incrementoMinimo : data.precioBase;
                setMontoPuja(minimo); // Seteamos el valor sugerido
                
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
        // --- Intentamos leer nuestro ID del Token ---
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

    // --- EFECTO PARA SIGNALR ---
    useEffect(() => {
        const connection = new HubConnectionBuilder()
            .withUrl("https://localhost:7109/hubs/subasta")
            .configureLogging(LogLevel.Information)
            .build();

        connection.start()
            .then(() => {
                console.log("Conectado a SignalR con éxito.");
                connection.invoke("UnirseASala", parseInt(id));

                connection.on("RecibirNuevaPuja", (nuevaPuja) => {
                    console.log("¡Alguien pujó!", nuevaPuja);

                    setSubasta((estadoAnterior) => {
                        if (!estadoAnterior) return estadoAnterior;

                        // ESCUDO ANTI-DUPLICADOS: Si la puja actual en pantalla ya tiene 
                        // el mismo monto que la que acaba de llegar, la ignoramos.
                        if (estadoAnterior.historialPujas.length > 0 && 
                            estadoAnterior.historialPujas[0].monto === nuevaPuja.monto) {
                            return estadoAnterior;
                        }

                        const pujaFormateada = {
                            monto: nuevaPuja.monto,
                            comprador: nuevaPuja.comprador,
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
 
        return () => {
            // Cerramos la conexión incondicionalmente. SignalR en el backend 
            // ya se encarga automáticamente de sacarte de la sala (grupo) al desconectarte.
            connection.stop();
        };
    }, [id]);

    // --- Sincronizar el input de oferta con la subasta ---
    useEffect(() => {
        if (subasta) {
            // Buscamos la oferta más alta de la lista actualizada
            const ofertaAlta = subasta.historialPujas.length > 0 
                ? subasta.historialPujas[0].monto 
                : subasta.precioBase;
            
            // Calculamos cuánto es lo mínimo que se puede ofertar ahora
            const minimoRequerido = ofertaAlta + subasta.incrementoMinimo;
            
            // Actualizamos la caja de texto
            setMontoPuja((montoActual) => {
                // Si la caja está vacía, o si el usuario tenía escrito un valor viejo 
                // (ej: 56000) que ahora ya no sirve, le forzamos el nuevo mínimo (57000).
                if (!montoActual || parseFloat(montoActual) < minimoRequerido) {
                    return minimoRequerido;
                }
                // Si el usuario justo estaba tipeando, no se lo borramos
                return montoActual;
            });
        }
    }, [subasta]); // El "trigger": se ejecuta cada vez que "subasta" cambia
    
    const handlePujar = async (e) => {
        e.preventDefault();
        setMensajePuja({ tipo: '', texto: '' });
        setEnviando(true);

        const token = localStorage.getItem('token');
        if (!token) {
            setMensajePuja({ tipo: 'danger', texto: 'Debes iniciar sesión para pujar.' });
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
                body: JSON.stringify({
                    // Ya no mandamos el compradorId. Solo mandamos el dinero.
                    monto: parseFloat(montoPuja)
                })
            });

            const data = await response.json();

            if (response.ok) {
                setMensajePuja({ tipo: 'success', texto: data.mensaje });
                obtenerDetalle(); // Recargamos para ver la nueva puja en el historial
            } else {
                // Acá va a saltar el texto: "Ya posees la oferta más alta..."
                setMensajePuja({ tipo: 'danger', texto: data.error || data.detail || 'Error al pujar.' });
            }
        } catch (error) {
            setMensajePuja({ tipo: 'danger', texto: 'Error de conexión con el servidor.' });
        } finally {
            setEnviando(false);
        }
    };

    if (cargando) return <div className="text-center mt-5"><h4>Cargando detalle...</h4></div>;
    if (error) return <div className="alert alert-danger mt-5 container">{error}</div>;
    if (!subasta) return null;

    return (
        <div className="container mt-5">
            <div className="mb-4">
                <Link 
                    to={rutaVolver} 
                    state={tabDeOrigen ? { tab: tabDeOrigen } : null}
                    className="btn btn-secondary btn-sm"
                >
                    &larr; {textoVolver}
                </Link>
            </div>

            <div className="row">
                {/* Columna Izquierda: Información de la Subasta */}
                <div className="col-md-8">
                    <div className="card shadow-sm mb-4">
                        <div className="card-body">
                            <div className="d-flex justify-content-between align-items-center mb-3">
                                <h2 className="card-title text-primary mb-0">{subasta.titulo}</h2>
                                <span className={`badge fs-6 ${subasta.estado === 'ACTIVA' ? 'bg-success' : 'bg-secondary'}`}>
                                    {subasta.estado}
                                </span>
                            </div>
                            
                            <p className="lead">{subasta.descripcion}</p>
                            
                            <hr />
                            
                            <div className="row text-center mt-4">
                                <div className="col-sm-4">
                                    <h6 className="text-muted">Precio Base</h6>
                                    <h5>${subasta.precioBase}</h5>
                                </div>
                                <div className="col-sm-4">
                                    <h6 className="text-muted">Incremento Mínimo</h6>
                                    <h5>${subasta.incrementoMinimo}</h5>
                                </div>
                                <div className="col-sm-4">
                                    <h6 className="text-muted">Vendedor</h6>
                                    <h5>{subasta.vendedor}</h5>
                                </div>
                            </div>
                        </div>
                        <div className="card-footer text-muted text-center">
                            Finaliza el: {formatearFechaLocal(subasta.fechaFin)}
                        </div>
                    </div>

                    {/* Historial de Pujas */}
                    <div className="card shadow-sm">
                        <div className="card-header bg-light">
                            <h5 className="mb-0">Historial de Pujas ({subasta.pujasTotal})</h5>
                        </div>
                        <ul className="list-group list-group-flush">
                            {subasta.historialPujas && subasta.historialPujas.length > 0 ? (
                                subasta.historialPujas.map((puja, index) => (
                                    <li key={index} className="list-group-item d-flex justify-content-between align-items-center">
                                        <div>
                                            <strong>{puja.comprador}</strong>
                                            <div className="text-muted small">{formatearFechaLocal(puja.fecha)}</div>
                                        </div>
                                        <span className="badge bg-primary rounded-pill fs-6">
                                            ${puja.monto}
                                        </span>
                                    </li>
                                ))
                            ) : (
                                <li className="list-group-item text-center text-muted py-4">
                                    Todavía no hay ofertas. ¡Sé el primero en pujar!
                                </li>
                            )}
                        </ul>
                    </div>
                </div>

                {/* Columna Derecha: Panel de Acción */}
                <div className="col-md-4 mt-4 mt-md-0">
                    <div className="card shadow-sm border-primary">
                        <div className="card-body text-center">
                            <h4 className="text-primary">Participar</h4>
                            <p className="text-muted small mb-4">Ingresá tu oferta superando la puja actual.</p>
                            
                            <div className="alert alert-info">
                                <strong>Oferta más alta actual:</strong><br/>
                                <h3>
                                    ${subasta.historialPujas.length > 0 
                                        ? subasta.historialPujas[0].monto 
                                        : subasta.precioBase}
                                </h3>
                            </div>

                            {/* Mensajes de feedback (Error o Éxito) */}
                            {mensajePuja.texto && (
                                <div className={`alert alert-${mensajePuja.tipo} small`}>
                                    {mensajePuja.texto}
                                </div>
                            )}

                            {/* Validamos si somos el dueño de la subasta */}
                            {subasta.vendedorId === miUsuarioId ? (
                                <div className="alert alert-warning mt-4">
                                    <i className="bi bi-info-circle me-2"></i>
                                    Esta es tu publicación. No podés pujar por tus propios artículos.
                                </div>
                            ) : (
                                <>
                                    <form onSubmit={handlePujar}>
                                        <div className="input-group mb-3">
                                            <span className="input-group-text">$</span>
                                            <input 
                                                type="number" 
                                                className="form-control form-control-lg" 
                                                value={montoPuja}
                                                onChange={(e) => setMontoPuja(e.target.value)}
                                                step="0.01"
                                                required
                                                disabled={enviando || subasta.estado !== 'ACTIVA'}
                                            />
                                        </div>
                                        <button 
                                            type="submit" 
                                            className="btn btn-primary w-100 btn-lg" 
                                            disabled={enviando || subasta.estado !== 'ACTIVA'}
                                        >
                                            {enviando ? 'Enviando...' : 'Confirmar Puja'}
                                        </button>
                                    </form>
                                    
                                    {subasta.estado !== 'ACTIVA' && (
                                        <div className="text-danger small mt-2">
                                            La subasta ya no se encuentra activa.
                                        </div>
                                    )}
                                </>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Detalle;