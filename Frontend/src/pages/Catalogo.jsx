import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { formatearFechaLocal } from '../utils/formatters';
import ContadorRegresivo from '../components/ContadorRegresivo';

const Catalogo = () => {
    const [subastas, setSubastas] = useState([]);
    const [cargando, setCargando] = useState(true);

    const [pagina, setPagina] = useState(1);
    const [paginacionInfo, setPaginacionInfo] = useState({});
    
    const [mostrarFiltros, setMostrarFiltros] = useState(false);

    // Estados "Borrador"
    const [inputEstado, setInputEstado] = useState('ACTIVA');
    const [inputCategoria, setInputCategoria] = useState('');
    const [inputOrden, setInputOrden] = useState('tiempo_restante');
    const [inputPrecioMin, setInputPrecioMin] = useState('');
    const [inputPrecioMax, setInputPrecioMax] = useState('');

    // Estados "Aplicados"
    const [filtroEstado, setFiltroEstado] = useState('ACTIVA');
    const [filtroCategoria, setFiltroCategoria] = useState('');
    const [filtroOrden, setFiltroOrden] = useState('tiempo_restante');
    const [filtroPrecioMin, setFiltroPrecioMin] = useState('');
    const [filtroPrecioMax, setFiltroPrecioMax] = useState('');

    useEffect(() => {
        const obtenerSubastas = async () => {
            setCargando(true);
            try {
                const params = new URLSearchParams();
                params.append('pagina', pagina);
                params.append('tamañoPagina', 6); 
                
                if (filtroEstado) params.append('estado', filtroEstado);
                if (filtroCategoria) params.append('categoriaId', filtroCategoria);
                if (filtroOrden) params.append('orden', filtroOrden);
                if (filtroPrecioMin) params.append('precioMin', filtroPrecioMin);
                if (filtroPrecioMax) params.append('precioMax', filtroPrecioMax);

                const response = await fetch(`https://localhost:7109/api/v1/auctions?${params.toString()}`);
                const data = await response.json();
                
                if (response.ok) {
                    setSubastas(data.items);
                    setPaginacionInfo(data.paginacion);
                } else {
                    console.error("Error al traer el catálogo:", data);
                }
            } catch (error) {
                console.error("Error de conexión:", error);
            } finally {
                setCargando(false);
            }
        };

        obtenerSubastas();
    }, [pagina, filtroEstado, filtroCategoria, filtroOrden, filtroPrecioMin, filtroPrecioMax]); 

    const handleAplicarFiltros = (e) => {
        e.preventDefault();
        setFiltroEstado(inputEstado);
        setFiltroCategoria(inputCategoria);
        setFiltroOrden(inputOrden);
        setFiltroPrecioMin(inputPrecioMin);
        setFiltroPrecioMax(inputPrecioMax);
        setPagina(1); 
    };

    // Nueva función para resetear todo a los valores por defecto
    const handleLimpiarFiltros = () => {
        // 1. Reseteamos lo que el usuario ve en las cajas
        setInputEstado('ACTIVA');
        setInputCategoria('');
        setInputOrden('tiempo_restante');
        setInputPrecioMin('');
        setInputPrecioMax('');

        // 2. Reseteamos lo que va a la API
        setFiltroEstado('ACTIVA');
        setFiltroCategoria('');
        setFiltroOrden('tiempo_restante');
        setFiltroPrecioMin('');
        setFiltroPrecioMax('');
        
        // 3. Volvemos a la página 1
        setPagina(1);
    };

    return (
        <div className="container mt-5">
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h2 className="text-primary mb-0">Catálogo de Subastas</h2>
                <button 
                    className="btn btn-outline-primary d-flex align-items-center gap-2"
                    onClick={() => setMostrarFiltros(!mostrarFiltros)}
                >
                    <i className="bi bi-filter"></i> 
                    {mostrarFiltros ? 'Ocultar Filtros' : 'Filtrar y Ordenar'}
                </button>
            </div>
            
            {mostrarFiltros && (
                <form onSubmit={handleAplicarFiltros} className="bg-light p-4 rounded shadow-sm mb-4 border">
                    <div className="row g-3 align-items-end mb-3">
                        <div className="col-md-4">
                            <label className="form-label fw-bold text-muted small">Estado</label>
                            <select className="form-select" value={inputEstado} onChange={(e) => setInputEstado(e.target.value)}>
                                <option value="">Todos los estados</option>
                                <option value="ACTIVA">Activas</option>
                                <option value="PROGRAMADA">Programadas</option>
                                <option value="FINALIZADA">Finalizadas</option>
                            </select>
                        </div>
                        <div className="col-md-4">
                            <label className="form-label fw-bold text-muted small">Categoría</label>
                            <select className="form-select" value={inputCategoria} onChange={(e) => setInputCategoria(e.target.value)}>
                                <option value="">Todas las categorías</option>
                                <option value="1">Tecnología</option>
                                <option value="2">Coleccionables</option>
                                <option value="3">Indumentaria</option>
                                <option value="4">Vehículos</option>
                            </select>
                        </div>
                        <div className="col-md-4">
                            <label className="form-label fw-bold text-muted small">Ordenar por</label>
                            <select className="form-select" value={inputOrden} onChange={(e) => setInputOrden(e.target.value)}>
                                <option value="tiempo_restante">Fecha de cierre</option>
                                <option value="mayor_puja">Mayor precio primero</option>
                            </select>
                        </div>
                    </div>
                    <div className="row g-3 align-items-end">
                        <div className="col-md-4">
                            <label className="form-label fw-bold text-muted small">Precio Mínimo ($)</label>
                            <input 
                                type="number" 
                                className="form-control" 
                                placeholder="Ej: 1000"
                                value={inputPrecioMin}
                                onChange={(e) => setInputPrecioMin(e.target.value)} 
                            />
                        </div>
                        <div className="col-md-4">
                            <label className="form-label fw-bold text-muted small">Precio Máximo ($)</label>
                            <input 
                                type="number" 
                                className="form-control" 
                                placeholder="Ej: 50000"
                                value={inputPrecioMax}
                                onChange={(e) => setInputPrecioMax(e.target.value)} 
                            />
                        </div>
                        <div className="col-md-4 d-flex gap-2">
                            <button type="button" className="btn btn-outline-secondary w-50 fw-bold" onClick={handleLimpiarFiltros}>
                                Limpiar
                            </button>
                            <button type="submit" className="btn btn-primary w-50 fw-bold">
                                <i className="bi bi-search me-2"></i> Aplicar
                            </button>
                        </div>
                    </div>
                </form>
            )}

            {cargando ? (
                <div className="text-center mt-5 py-5">
                    <div className="spinner-border text-primary" role="status"></div>
                    <h5 className="mt-3 text-muted">Buscando subastas...</h5>
                </div>
            ) : (
                <>
                    <div className="row g-4">
                        {subastas.map((subasta) => (
                            <div className="col-12 col-md-6 col-lg-4" key={subasta.id}>
                                <div className="card h-100 shadow-sm">
                                    {subasta.urlImagen && (
                                        <img 
                                            src={subasta.urlImagen} 
                                            className="card-img-top border-bottom" 
                                            alt={subasta.titulo} 
                                            style={{ height: '200px', objectFit: 'cover' }} 
                                        />
                                    )}
                                    <div className="card-body">
                                        {/* Título y Estado */}
                                        <div className="d-flex justify-content-between align-items-start mb-2">
                                            <h5 className="card-title text-truncate mb-0" title={subasta.titulo}>
                                                {subasta.titulo}
                                            </h5>
                                            <span className={`badge ${subasta.estado === 'ACTIVA' ? 'bg-success' : 'bg-secondary'}`}>
                                                {subasta.estado}
                                            </span>
                                        </div>
                                        
                                        {/* Categoría (Asegurate de que tu backend envíe esta propiedad en el JSON) */}
                                        <p className="text-muted small mb-3">
                                            <i className="bi bi-tag-fill me-1"></i> 
                                            {subasta.categoria || 'Sin categoría'}
                                        </p>

                                        {/* Ofertas */}
                                        <div className="bg-light p-2 rounded mb-3 text-center">
                                            <p className="card-text mb-1 text-muted small">Oferta más alta</p>
                                            <h4 className="text-primary mb-0">${subasta.ofertaMasAlta}</h4>
                                            <p className="card-text text-muted small mt-1 mb-0">
                                                ({subasta.cantidadOfertas} pujas realizadas)
                                            </p>
                                        </div>

                                        <Link to={`/subasta/${subasta.id}`} className="btn btn-outline-primary w-100 fw-bold">
                                            Ver Detalle
                                        </Link>
                                    </div>
                                    
                                    {/* Contador Regresivo Inteligente */}
                                    <div className="card-footer bg-white border-top-0 pb-3">
                                        <ContadorRegresivo 
                                            fechaInicio={subasta.fechaInicio} 
                                            fechaFin={subasta.fechaFin} 
                                            estado={subasta.estado} 
                                        />
                                    </div>
                                </div>
                            </div>
                        ))}

                        {subastas.length === 0 && (
                            <div className="col-12">
                                <div className="alert alert-info">
                                    <i className="bi bi-info-circle me-2"></i>
                                    No se encontraron subastas con los filtros seleccionados.
                                </div>
                            </div>
                        )}
                    </div>

                    {paginacionInfo.totalPaginas > 1 && (
                        <div className="d-flex justify-content-center align-items-center mt-5 mb-4">
                            <button 
                                className="btn btn-outline-primary me-3" 
                                disabled={pagina === 1}
                                onClick={() => setPagina(p => p - 1)}
                            >
                                &laquo; Anterior
                            </button>
                            <span className="fw-bold text-muted">
                                Página {paginacionInfo.paginaActual} de {paginacionInfo.totalPaginas}
                            </span>
                            <button 
                                className="btn btn-outline-primary ms-3" 
                                disabled={pagina === paginacionInfo.totalPaginas}
                                onClick={() => setPagina(p => p + 1)}
                            >
                                Siguiente &raquo;
                            </button>
                        </div>
                    )}
                </>
            )}
        </div>
    );
};

export default Catalogo;