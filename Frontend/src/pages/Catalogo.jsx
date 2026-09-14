import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import ContadorRegresivo from '../components/ContadorRegresivo';
import ImagenTarjeta from '../components/ImagenTarjeta';

const Catalogo = () => {
    const [subastas, setSubastas] = useState([]);
    const [cargando, setCargando] = useState(true);

    const [pagina, setPagina] = useState(() => parseInt(sessionStorage.getItem('cat_pagina')) || 1);
    const [paginacionInfo, setPaginacionInfo] = useState({});

    // Estados "Borrador" inicializados desde sessionStorage
    const [inputEstado, setInputEstado] = useState(() => sessionStorage.getItem('cat_inputEstado') ?? 'ACTIVA');
    const [inputCategoria, setInputCategoria] = useState(() => sessionStorage.getItem('cat_inputCategoria') || '');
    const [inputOrden, setInputOrden] = useState(() => sessionStorage.getItem('cat_inputOrden') || 'tiempo_restante');
    const [inputPrecioMin, setInputPrecioMin] = useState(() => sessionStorage.getItem('cat_inputPrecioMin') || '');
    const [inputPrecioMax, setInputPrecioMax] = useState(() => sessionStorage.getItem('cat_inputPrecioMax') || '');

    // Estados "Aplicados" inicializados desde sessionStorage
    const [filtroEstado, setFiltroEstado] = useState(() => sessionStorage.getItem('cat_filtroEstado') ?? 'ACTIVA');
    const [filtroCategoria, setFiltroCategoria] = useState(() => sessionStorage.getItem('cat_filtroCategoria') || '');
    const [filtroOrden, setFiltroOrden] = useState(() => sessionStorage.getItem('cat_filtroOrden') || 'tiempo_restante');
    const [filtroPrecioMin, setFiltroPrecioMin] = useState(() => sessionStorage.getItem('cat_filtroPrecioMin') || '');
    const [filtroPrecioMax, setFiltroPrecioMax] = useState(() => sessionStorage.getItem('cat_filtroPrecioMax') || '');

    useEffect(() => {
        const context = { isMounted: true };

        const timeoutId = setTimeout(() => {
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
                    
                    if (response.ok && context.isMounted) {
                        setSubastas(data.items);
                        setPaginacionInfo(data.paginacion);
                    }
                } catch (error) {
                    console.error("Error de conexión:", error);
                } finally {
                    if (context.isMounted) setCargando(false);
                }
            };

            obtenerSubastas();
        }, 250);

        return () => {
            context.isMounted = false;
            clearTimeout(timeoutId);
        };
    }, [pagina, filtroEstado, filtroCategoria, filtroOrden, filtroPrecioMin, filtroPrecioMax]); 

    const handleAplicarFiltros = (e) => {
        e.preventDefault();
        setFiltroEstado(inputEstado);
        setFiltroCategoria(inputCategoria);
        setFiltroOrden(inputOrden);
        setFiltroPrecioMin(inputPrecioMin);
        setFiltroPrecioMax(inputPrecioMax);
        setPagina(1); 

        sessionStorage.setItem('cat_inputEstado', inputEstado);
        sessionStorage.setItem('cat_inputCategoria', inputCategoria);
        sessionStorage.setItem('cat_inputOrden', inputOrden);
        sessionStorage.setItem('cat_inputPrecioMin', inputPrecioMin);
        sessionStorage.setItem('cat_inputPrecioMax', inputPrecioMax);

        sessionStorage.setItem('cat_filtroEstado', inputEstado);
        sessionStorage.setItem('cat_filtroCategoria', inputCategoria);
        sessionStorage.setItem('cat_filtroOrden', inputOrden);
        sessionStorage.setItem('cat_filtroPrecioMin', inputPrecioMin);
        sessionStorage.setItem('cat_filtroPrecioMax', inputPrecioMax);
        sessionStorage.setItem('cat_pagina', '1');
    };

    const handleLimpiarFiltros = () => {
        setInputEstado('ACTIVA');
        setInputCategoria('');
        setInputOrden('tiempo_restante');
        setInputPrecioMin('');
        setInputPrecioMax('');

        setFiltroEstado('ACTIVA');
        setFiltroCategoria('');
        setFiltroOrden('tiempo_restante');
        setFiltroPrecioMin('');
        setFiltroPrecioMax('');
        
        setPagina(1);

        sessionStorage.setItem('cat_inputEstado', 'ACTIVA');
        sessionStorage.setItem('cat_inputCategoria', '');
        sessionStorage.setItem('cat_inputOrden', 'tiempo_restante');
        sessionStorage.setItem('cat_inputPrecioMin', '');
        sessionStorage.setItem('cat_inputPrecioMax', '');

        sessionStorage.setItem('cat_filtroEstado', 'ACTIVA');
        sessionStorage.setItem('cat_filtroCategoria', '');
        sessionStorage.setItem('cat_filtroOrden', 'tiempo_restante');
        sessionStorage.setItem('cat_filtroPrecioMin', '');
        sessionStorage.setItem('cat_filtroPrecioMax', '');
        sessionStorage.setItem('cat_pagina', '1');
    };

    return (
        <div className="container mt-5">
            {/* Banner */}
            <div className="p-4 p-md-5 mb-4 rounded text-bg-dark shadow" style={{ background: 'linear-gradient(45deg, #1a2a6c, #b21f1f, #fdbb2d)', backgroundSize: 'cover' }}>
                <div className="col-md-8 px-0">
                    <h1 className="display-4 fst-italic fw-bold text-white mb-3">Encontrá oportunidades únicas</h1>
                    <p className="lead my-3 fw-light">Participá en tiempo real y llevate lo que siempre quisiste al mejor precio. Nuestra plataforma garantiza transparencia y seguridad en cada puja.</p>
                </div>
            </div>

            {/* Cabecera - Filtros y Publicar */}
            <div className="d-flex justify-content-between align-items-center mb-4">
                <div className="d-flex align-items-center gap-3">
                    <button 
                        className="btn btn-primary d-flex align-items-center justify-content-center shadow-sm"
                        type="button"
                        data-bs-toggle="offcanvas" 
                        data-bs-target="#panelFiltros" 
                        aria-controls="panelFiltros"
                        title="Abrir Filtros"
                        style={{ width: '42px', height: '42px' }}
                    >
                        <i className="bi bi-funnel-fill fs-5 text-white"></i>
                    </button>
                    <h2 className="text-dark mb-0">Catálogo de Subastas</h2>
                </div>

                <Link to="/crear-subasta" className="btn btn-warning fw-bold shadow-sm hover-animado d-none d-md-flex align-items-center gap-2">
                    <i className="bi bi-plus-circle-fill fs-5"></i> Publicar Subasta
                </Link>
                <Link to="/crear-subasta" className="btn btn-warning fw-bold shadow-sm d-md-none d-flex align-items-center justify-content-center" style={{ width: '42px', height: '42px' }}>
                    <i className="bi bi-plus-circle-fill fs-5"></i>
                </Link>
            </div>

            {/* Offcanvas - Panel lateral independiente) */}
            <div className="offcanvas offcanvas-start border-0 shadow" tabIndex="-1" id="panelFiltros" aria-labelledby="panelFiltrosLabel">
                <div className="offcanvas-header bg-light border-bottom">
                    <h5 className="offcanvas-title text-primary fw-bold d-flex align-items-center gap-2" id="panelFiltrosLabel">
                        <i className="bi bi-sliders"></i> Filtros de Búsqueda
                    </h5>
                    <button type="button" className="btn-close" data-bs-dismiss="offcanvas" aria-label="Cerrar"></button>
                </div>
                <div className="offcanvas-body">
                    <form onSubmit={handleAplicarFiltros}>
                        <div className="mb-3">
                            <label className="form-label fw-bold text-muted small">Estado</label>
                            <select className="form-select" value={inputEstado} onChange={(e) => setInputEstado(e.target.value)}>
                                <option value="">Todos los estados</option>
                                <option value="ACTIVA">Activas</option>
                                <option value="PROGRAMADA">Programadas</option>
                                <option value="FINALIZADA">Finalizadas</option>
                            </select>
                        </div>
                        <div className="mb-3">
                            <label className="form-label fw-bold text-muted small">Categoría</label>
                            <select className="form-select" value={inputCategoria} onChange={(e) => setInputCategoria(e.target.value)}>
                                <option value="">Todas las categorías</option>
                                <option value="1">Tecnología</option>
                                <option value="2">Coleccionables</option>
                                <option value="3">Indumentaria</option>
                                <option value="4">Vehículos</option>
                                <option value="5">Otros</option>
                            </select>
                        </div>
                        <div className="mb-4">
                            <label className="form-label fw-bold text-muted small">Ordenar por</label>
                            <select className="form-select" value={inputOrden} onChange={(e) => setInputOrden(e.target.value)}>
                                <option value="tiempo_restante">Fecha de cierre</option>
                                <option value="mayor_puja">Mayor precio primero</option>
                            </select>
                        </div>
                        <div className="row g-2 mb-5">
                            <div className="col-6">
                                <label className="form-label fw-bold text-muted small">Min ($)</label>
                                <input 
                                    type="number" 
                                    className="form-control" 
                                    placeholder="Ej: 1000" 
                                    value={inputPrecioMin} 
                                    onChange={(e) => setInputPrecioMin(e.target.value)} 
                                    onKeyDown={(e) => ["e", "E", "+", "-"].includes(e.key) && e.preventDefault()}
                                />
                            </div>
                            <div className="col-6">
                                <label className="form-label fw-bold text-muted small">Max ($)</label>
                                <input 
                                    type="number" 
                                    className="form-control" 
                                    placeholder="Ej: 50000" 
                                    value={inputPrecioMax} 
                                    onChange={(e) => setInputPrecioMax(e.target.value)} 
                                    onKeyDown={(e) => ["e", "E", "+", "-"].includes(e.key) && e.preventDefault()}
                                />
                            </div>
                        </div>
                        
                        <div className="d-flex flex-column gap-2">
                            <button type="submit" className="btn btn-primary fw-bold" data-bs-dismiss="offcanvas">
                                <i className="bi bi-search me-2"></i> Aplicar Filtros
                            </button>
                            <button type="button" className="btn btn-outline-secondary fw-bold" onClick={handleLimpiarFiltros} data-bs-dismiss="offcanvas">
                                Limpiar todo
                            </button>
                        </div>
                    </form>
                </div>
            </div>

            {/* Grilla de tarjetas */}
            <div className="row">
                <div className="col-12">
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
                                        <div className="card h-100 shadow-sm border-0 hover-animado">
                                            <ImagenTarjeta 
                                                src={subasta.urlImagen} 
                                                alt={subasta.titulo} 
                                                height="200px" 
                                            />
                                            <div className="card-body">
                                                <div className="d-flex justify-content-between align-items-start mb-2">
                                                    <h5 className="card-title text-truncate mb-0" title={subasta.titulo}>
                                                        {subasta.titulo}
                                                    </h5>
                                                    <span className={`badge ${subasta.estado === 'ACTIVA' ? 'bg-success' : 'bg-secondary'}`}>
                                                        {subasta.estado}
                                                    </span>
                                                </div>
                                                
                                                <p className="text-muted small mb-3">
                                                    <i className="bi bi-tag-fill me-1"></i> 
                                                    {subasta.categoria || 'Sin categoría'}
                                                </p>

                                                <div className="bg-light p-2 rounded mb-3 text-center">
                                                    <p className="card-text mb-1 text-muted small">Oferta más alta</p>
                                                    <h4 className="mb-0 precio-destacado">${subasta.ofertaMasAlta}</h4>
                                                    <p className="card-text text-muted small mt-1 mb-0">
                                                        ({subasta.cantidadOfertas} pujas realizadas)
                                                    </p>
                                                </div>

                                                <Link to={`/subasta/${subasta.id}`} className="btn btn-outline-primary w-100 fw-bold">
                                                    Ver Detalle
                                                </Link>
                                            </div>
                                            
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
            </div>
        </div>
    );
};

export default Catalogo;