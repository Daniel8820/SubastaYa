import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { formatearFechaLocal } from '../utils/formatters';

const Catalogo = () => {
    // Guardamos las subastas que vienen del backend
    const [subastas, setSubastas] = useState([]);
    const [cargando, setCargando] = useState(true);

    // useEffect se ejecuta automáticamente al abrir la pantalla
    useEffect(() => {
        const obtenerSubastas = async () => {
            try {
                // Endpoint del backend que devuelve el catálogo de subastas
                const response = await fetch('https://localhost:7109/api/v1/auctions');
                const data = await response.json();
                
                if (response.ok) {
                    // El endpoint devuelve { paginacion: {...}, items: [...] }
                    // Guardamos solo el array de items
                    setSubastas(data.items);
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
    }, []); // Los corchetes vacíos indican que solo se ejecuta 1 vez al cargar

    if (cargando) {
        return <div className="text-center mt-5"><h4>Cargando subastas...</h4></div>;
    }

    return (
        <div className="container mt-5">
            <h2 className="text-primary mb-4">Catálogo de Subastas</h2>
            
            <div className="row g-4">
                {/* Recorremos el array de subastas y armamos una "Card" de Bootstrap por cada una */}
                {subastas.map((subasta) => (
                    <div className="col-12 col-md-6 col-lg-4" key={subasta.id}>
                        <div className="card h-100 shadow-sm">
                            <div className="card-body">
                                <h5 className="card-title">{subasta.titulo}</h5>
                                <div className="mb-2">
                                    <span className={`badge ${subasta.estado === 'ACTIVA' ? 'bg-success' : 'bg-secondary'}`}>
                                        {subasta.estado}
                                    </span>
                                </div>
                                <p className="card-text mb-1">
                                    <strong>Oferta más alta:</strong> ${subasta.ofertaMasAlta}
                                </p>
                                <p className="card-text text-muted small mb-3">
                                    Pujas hasta ahora: {subasta.cantidadOfertas}
                                </p>
                                <Link to={`/subasta/${subasta.id}`} className="btn btn-outline-primary w-100">
                                    Ver Detalle
                                </Link>
                            </div>
                            <div className="card-footer bg-transparent text-muted small">
                                Cierra el: {formatearFechaLocal(subasta.fechaFin)}
                            </div>
                        </div>
                    </div>
                ))}

                {subastas.length === 0 && (
                    <div className="col-12">
                        <div className="alert alert-info">No hay subastas disponibles en este momento.</div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default Catalogo;