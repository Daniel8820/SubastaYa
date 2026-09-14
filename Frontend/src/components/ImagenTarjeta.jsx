import { useState } from 'react';

const ImagenTarjeta = ({ src, alt, height = '200px' }) => {
    const [cargada, setCargada] = useState(false);
    const [error, setError] = useState(false);

    if (!src) return null;

    return (
        <div 
            className="bg-light border-bottom d-flex flex-column align-items-center justify-content-center position-relative w-100" 
            style={{ height: height }}
        >
            {!cargada && !error && (
                <div className="text-muted text-center position-absolute">
                    <div className="spinner-border spinner-border-sm text-primary mb-1" role="status"></div>
                    <div style={{ fontSize: '0.75rem' }} className="fw-bold">Cargando...</div>
                </div>
            )}
            {error && (
                <div className="text-muted text-center position-absolute">
                    <i className="bi bi-image text-secondary mb-1" style={{ fontSize: '1.5rem' }}></i>
                    <div style={{ fontSize: '0.75rem' }} className="fw-bold">No disponible</div>
                </div>
            )}
            <img 
                src={src} 
                alt={alt} 
                className={`card-img-top w-100 ${cargada && !error ? 'd-block' : 'd-none'}`} 
                style={{ height: height, objectFit: 'cover' }} 
                onLoad={() => setCargada(true)}
                onError={() => setError(true)}
            />
        </div>
    );
};

export default ImagenTarjeta;