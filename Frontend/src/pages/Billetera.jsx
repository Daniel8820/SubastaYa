import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { formatearFechaLocal } from '../utils/formatters';
import toast from 'react-hot-toast';

const Billetera = () => {
    const [saldo, setSaldo] = useState(null);
    const [historial, setHistorial] = useState([]);
    const [cargando, setCargando] = useState(true);
    const [montoDeposito, setMontoDeposito] = useState('');
    const [depositando, setDepositando] = useState(false);
    const navigate = useNavigate();

    // Usamos un objeto de contexto para pasarlo por referencia y saber si el componente sigue vivo
    const cargarDatosBilletera = async (context = { isMounted: true }) => {
        const token = localStorage.getItem('token');
        if (!token) {
            navigate('/login');
            return;
        }

        try {
            const [resSaldo, resHistorial] = await Promise.all([
                fetch('https://localhost:7109/api/wallet/balance', {
                    headers: { 'Authorization': `Bearer ${token}` }
                }),
                fetch('https://localhost:7109/api/wallet/history', {
                    headers: { 'Authorization': `Bearer ${token}` }
                })
            ]);

            if (resSaldo.ok && resHistorial.ok) {
                const dataSaldo = await resSaldo.json();
                const dataHistorial = await resHistorial.json();
                
                // Solo actualizamos la pantalla si el usuario sigue en la Billetera
                if (context.isMounted) {
                    setSaldo(dataSaldo);
                    setHistorial(dataHistorial);
                }
            } else {
                if (context.isMounted) toast.error('Error al cargar los datos de la billetera.');
            }
        } catch (error) {
            if (context.isMounted) toast.error('Error de conexión con el servidor.');
        } finally {
            if (context.isMounted) setCargando(false);
        }
    };

    useEffect(() => {
        const context = { isMounted: true };
        
        // Retrasamos 250ms la carga de datos para evitar que el spinner parpadee si la respuesta es muy rápida
        const timeoutId = setTimeout(() => {
            cargarDatosBilletera(context);
        }, 250);

        return () => {
            context.isMounted = false; 
            clearTimeout(timeoutId);   
        };
    }, [navigate]);

    const handleDepositar = async (e) => {
        e.preventDefault();
        if (montoDeposito <= 0) {
            toast.error("El monto debe ser mayor a cero.");
            return;
        }

        setDepositando(true);
        const token = localStorage.getItem('token');

        try {
            const response = await fetch('https://localhost:7109/api/wallet/deposit', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ monto: parseFloat(montoDeposito) })
            });

            const data = await response.json();

            if (response.ok) {
                toast.success(data.mensaje || 'Depósito simulado con éxito.');
                setMontoDeposito('');
                cargarDatosBilletera();
            } else {
                toast.error(data.error || data.detail || 'Error al depositar.');
            }
        } catch (error) {
            toast.error('Error de conexión con el servidor.');
        } finally {
            setDepositando(false);
        }
    };

    // Diccionario visual para los tipos de transacciones
    const getBadgeTransaccion = (tipo) => {
        switch (tipo) {
            case 'DEPOSITO': return <span className="badge bg-success">Depósito</span>;
            case 'ACREDITACION_VENTA': return <span className="badge bg-success">Venta Adjudicada</span>;
            case 'RETENCION': return <span className="badge bg-warning text-dark">Retención en Garantía</span>;
            case 'LIBERACION': return <span className="badge bg-info text-dark">Garantía Liberada</span>;
            case 'PAGO_SUBASTA': return <span className="badge bg-danger">Pago por Subasta</span>;
            default: return <span className="badge bg-secondary">{tipo}</span>;
        }
    };

    if (cargando) return (
        <div className="text-center mt-5 py-5">
            <div className="spinner-border text-primary" role="status"></div>
            <h5 className="mt-3 text-muted">Cargando billetera...</h5>
        </div>
    );
    if (!saldo) return null;

    return (
        <div className="container mt-4 mb-5">
            <h2 className="mb-4 text-dark"><i className="bi bi-wallet2 me-2"></i>Mi Billetera</h2>

            {/* Panel de Saldos */}
            <div className="row g-4 mb-5">
                <div className="col-md-4">
                    <div className="card shadow-sm border-0 bg-light h-100">
                        <div className="card-body text-center">
                            <h6 className="text-muted text-uppercase mb-2">Saldo Total</h6>
                            <h2 className="mb-0 text-dark">${saldo.total}</h2>
                            <small className="text-muted">Fondos totales en la cuenta</small>
                        </div>
                    </div>
                </div>
                <div className="col-md-4">
                    <div className="card shadow-sm border-0 bg-warning bg-opacity-10 h-100">
                        <div className="card-body text-center">
                            <h6 className="text-warning text-uppercase mb-2" style={{filter: 'brightness(0.7)'}}>En Garantía</h6>
                            <h2 className="mb-0 text-warning" style={{filter: 'brightness(0.8)'}}>${saldo.retenido}</h2>
                            <small className="text-muted">Bloqueado por pujas activas</small>
                        </div>
                    </div>
                </div>
                <div className="col-md-4">
                    <div className="card shadow-sm border-0 bg-success bg-opacity-10 h-100">
                        <div className="card-body text-center">
                            <h6 className="text-success text-uppercase mb-2">Saldo Disponible</h6>
                            <h2 className="mb-0 text-success">${saldo.disponible}</h2>
                            <small className="text-muted">Poder de compra actual</small>
                        </div>
                    </div>
                </div>
            </div>

            <div className="row">
                {/* Columna izquierda - Historial */}
                <div className="col-lg-8 order-2 order-lg-1">
                    <div className="card shadow-sm">
                        <div className="card-header bg-white py-3">
                            <h5 className="mb-0"><i className="bi bi-card-list me-2"></i>Historial de Movimientos</h5>
                        </div>
                        <div className="table-responsive">
                            <table className="table table-hover align-middle mb-0">
                                <thead className="table-light">
                                    <tr>
                                        <th>Fecha</th>
                                        <th>Tipo de Movimiento</th>
                                        <th>Monto</th>
                                        <th>Subasta Relacionada</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {historial.length === 0 ? (
                                        <tr>
                                            <td colSpan="4" className="text-center py-4 text-muted">
                                                Aún no tienes movimientos registrados en tu billetera.
                                            </td>
                                        </tr>
                                    ) : (
                                        historial.map(tx => (
                                            <tr key={tx.id}>
                                                <td className="text-muted small">{formatearFechaLocal(tx.fecha)}</td>
                                                <td>{getBadgeTransaccion(tx.tipo)}</td>
                                                <td className="fw-bold text-dark">${tx.monto}</td>
                                                <td>
                                                    {tx.subastaId ? (
                                                        <a href={`/subasta/${tx.subastaId}`} className="text-decoration-none">
                                                            Subasta #{tx.subastaId}
                                                        </a>
                                                    ) : (
                                                        <span className="text-muted small">N/A</span>
                                                    )}
                                                </td>
                                            </tr>
                                        ))
                                    )}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>

                {/* Columna derecha - Formulario de carga */}
                <div className="col-lg-4 order-1 order-lg-2 mb-4 mb-lg-0">
                    <div className="card shadow-sm border-primary">
                        <div className="card-body">
                            <h5 className="card-title text-primary mb-3">
                                <i className="bi bi-cash-stack me-2"></i>Ingresar Dinero
                            </h5>
                            <p className="small text-muted mb-4">
                                Carga dinero a tu billetera para participar en las subastas.
                            </p>
                            <form onSubmit={handleDepositar}>
                                <div className="input-group mb-3">
                                    <span className="input-group-text">$</span>
                                    <input 
                                        type="number" 
                                        className="form-control form-control-lg fw-bold" 
                                        placeholder="Ej: 5000"
                                        value={montoDeposito}
                                        onChange={(e) => setMontoDeposito(e.target.value)}
                                        onKeyDown={(e) => ["e", "E", "+", "-"].includes(e.key) && e.preventDefault()}
                                        step="0.01"
                                        min="1"
                                        required
                                        disabled={depositando}
                                    />
                                </div>
                                <button 
                                    type="submit" 
                                    className="btn btn-primary w-100 fw-bold"
                                    disabled={depositando}
                                >
                                    {depositando ? 'Acreditando...' : 'Cargar Saldo'}
                                </button>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Billetera;