import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

const CrearSubasta = () => {
    const navigate = useNavigate();

    const [titulo, setTitulo] = useState('');
    const [descripcion, setDescripcion] = useState('');
    const [precioBase, setPrecioBase] = useState('');
    const [incrementoMinimo, setIncrementoMinimo] = useState('');
    const [categoriaId, setCategoriaId] = useState('1'); 
    
    // NUEVO: Estado para el archivo físico de la imagen
    const [imagenArchivo, setImagenArchivo] = useState(null);
    const [previsualizacion, setPrevisualizacion] = useState('');
    
    const [modoInicio, setModoInicio] = useState(''); 
    const [fechaInicio, setFechaInicio] = useState('');
    const [fechaFin, setFechaFin] = useState('');

    const [error, setError] = useState('');
    const [enviando, setEnviando] = useState(false);
    const [pasoActual, setPasoActual] = useState(''); // Para darle feedback al usuario

    // Función para manejar la selección del archivo y mostrar una miniatura
    const handleImagenChange = (e) => {
        const file = e.target.files[0];
        if (file) {
            setImagenArchivo(file);
            setPrevisualizacion(URL.createObjectURL(file));
        }
    };

    const handlePublicar = async (e) => {
        e.preventDefault();
        setError('');
        setEnviando(true);

        const token = localStorage.getItem('token');
        if (!token) {
            navigate('/login');
            return;
        }

        if (!imagenArchivo) {
            setError('Por favor, seleccioná una imagen para la subasta.');
            setEnviando(false);
            return;
        }

        try {
            // --- FASE 1: Subir imagen a ImgBB ---
            setPasoActual('Subiendo imagen al servidor externo...');
            
            const apiKey = import.meta.env.VITE_IMGBB_API_KEY; 
            
            // 1. Validamos que Vite haya cargado la variable de entorno
            if (!apiKey) {
                throw new Error("Falta la API Key. Detené la terminal de React (Ctrl+C) y volvé a levantarla.");
            }
            
            const formData = new FormData();
            // A veces ImgBB prefiere recibir la Key adentro del FormData en lugar de la URL
            formData.append('key', apiKey); 
            formData.append('image', imagenArchivo);
            
            const imgbbResponse = await fetch('https://api.imgbb.com/1/upload', {
                method: 'POST',
                body: formData
            });

            const imgbbData = await imgbbResponse.json();

            // 2. Si ImgBB la rechaza, leemos exactamente por qué fue
            if (!imgbbResponse.ok || !imgbbData.success) {
                console.error("Detalle del error de ImgBB:", imgbbData);
                throw new Error(imgbbData.error?.message || 'El servidor de imágenes rechazó el archivo.');
            }

            // Capturamos la URL pública definitiva
            const urlImagenFinal = imgbbData.data.url;

            // --- FASE 2: Registrar subasta en el backend C# ---
            setPasoActual('Registrando subasta en la base de datos...');

            let fechaInicioFinal = modoInicio === 'inmediata' 
                ? new Date().toISOString() 
                : new Date(fechaInicio).toISOString();

            const response = await fetch('https://localhost:7109/api/v1/auctions', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    titulo,
                    descripcion,
                    urlImagen: urlImagenFinal, // Inyectamos la URL que nos devolvió ImgBB
                    precioBase: parseFloat(precioBase),
                    incrementoMinimo: parseFloat(incrementoMinimo),
                    fechaInicio: fechaInicioFinal, 
                    fechaFin: new Date(fechaFin).toISOString(), 
                    categoriaId: parseInt(categoriaId)
                })
            });

            const data = await response.json();

            if (response.ok) {
                navigate(`/subasta/${data.subastaId}`);
            } else {
                setError(data.detail || data.error || 'Ocurrió un error al crear la subasta.');
            }
        } catch (err) {
            setError(err.message || 'Error de conexión con el servidor.');
        } finally {
            setEnviando(false);
            setPasoActual('');
        }
    };

    return (
        <div className="container mt-4 mb-5">
            <div className="row justify-content-center">
                <div className="col-md-10 col-lg-8">
                    <div className="card shadow-sm border-primary">
                        <div className="card-header bg-primary text-white">
                            <h4 className="mb-0 text-center"><i className="bi bi-tag-fill me-2"></i>Publicar nueva Subasta</h4>
                        </div>
                        <div className="card-body p-4">
                            {error && <div className="alert alert-danger">{error}</div>}

                            <form onSubmit={handlePublicar}>
                                <div className="mb-3">
                                    <label className="form-label fw-bold">Título del artículo</label>
                                    <input type="text" className="form-control" value={titulo} onChange={(e) => setTitulo(e.target.value)} required disabled={enviando} placeholder="Ej: Notebook HP EliteBook" />
                                </div>

                                <div className="mb-3">
                                    <label className="form-label fw-bold">Descripción detallada</label>
                                    <textarea className="form-control" rows="3" value={descripcion} onChange={(e) => setDescripcion(e.target.value)} required disabled={enviando} placeholder="Condición, detalles, especificaciones..."></textarea>
                                </div>

                                <div className="row mb-3">
                                    <div className="col-md-6">
                                        <label className="form-label fw-bold">Categoría</label>
                                        <select className="form-select" value={categoriaId} onChange={(e) => setCategoriaId(e.target.value)} required disabled={enviando}>
                                            <option value="1">Tecnología</option>
                                            <option value="2">Coleccionables</option>
                                            <option value="3">Indumentaria</option>
                                            <option value="4">Vehículos</option>
                                        </select>
                                    </div>
                                    
                                    {/* Zona de Carga de Imagen Física */}
                                    <div className="col-md-6 mt-3 mt-md-0">
                                        <label className="form-label fw-bold">Fotografía del Artículo</label>
                                        <input 
                                            type="file" 
                                            className="form-control" 
                                            accept="image/png, image/jpeg, image/webp" 
                                            onChange={handleImagenChange} 
                                            required 
                                            disabled={enviando} 
                                        />
                                        {previsualizacion && (
                                            <div className="mt-2 text-center bg-light p-2 rounded border">
                                                <img src={previsualizacion} alt="Previsualización" className="img-fluid rounded" style={{maxHeight: '120px', objectFit: 'cover'}} />
                                            </div>
                                        )}
                                    </div>
                                </div>

                                <div className="row mb-4">
                                    <div className="col-md-6">
                                        <label className="form-label fw-bold">Precio Base ($)</label>
                                        <input type="number" step="0.01" min="1" className="form-control" value={precioBase} onChange={(e) => setPrecioBase(e.target.value)} required disabled={enviando} />
                                    </div>
                                    <div className="col-md-6 mt-3 mt-md-0">
                                        <label className="form-label fw-bold">Incremento Mín. ($)</label>
                                        <input type="number" step="0.01" min="1" className="form-control" value={incrementoMinimo} onChange={(e) => setIncrementoMinimo(e.target.value)} required disabled={enviando} />
                                    </div>
                                </div>

                                <div className="row mb-5 bg-light p-3 rounded border">
                                    <div className="col-md-4">
                                        <label className="form-label fw-bold">Tipo de publicación</label>
                                        <select 
                                            className="form-select border-secondary" 
                                            value={modoInicio} 
                                            onChange={(e) => setModoInicio(e.target.value)} 
                                            required 
                                            disabled={enviando}
                                        >
                                            {modoInicio === '' && <option value="" disabled>---</option>}
                                            <option value="inmediata">Inmediata</option>
                                            <option value="programada">Programada</option>
                                        </select>
                                    </div>
                                    <div className="col-md-4 mt-3 mt-md-0">
                                        <label className="form-label fw-bold text-muted">Fecha Inicio</label>
                                        <input 
                                            type="datetime-local" 
                                            className="form-control" 
                                            style={{ colorScheme: 'light' }} 
                                            value={modoInicio === 'programada' ? fechaInicio : ''} 
                                            onChange={(e) => setFechaInicio(e.target.value)} 
                                            required={modoInicio === 'programada'} 
                                            disabled={modoInicio !== 'programada' || enviando} 
                                        />
                                    </div>
                                    <div className="col-md-4 mt-3 mt-md-0">
                                        <label className="form-label fw-bold">Fecha Fin</label>
                                        <input 
                                            type="datetime-local" 
                                            className="form-control" 
                                            style={{ colorScheme: 'light' }} 
                                            value={fechaFin} 
                                            onChange={(e) => setFechaFin(e.target.value)} 
                                            required={modoInicio !== ''} 
                                            disabled={modoInicio === '' || enviando} 
                                        />
                                    </div>
                                </div>

                                <button type="submit" className="btn btn-primary w-100 btn-lg" disabled={enviando}>
                                    {enviando ? (
                                        <><span className="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>{pasoActual}</>
                                    ) : 'Crear Subasta'}
                                </button>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CrearSubasta;