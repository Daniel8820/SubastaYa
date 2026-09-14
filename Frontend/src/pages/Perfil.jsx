import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';

const Perfil = () => {
    const navigate = useNavigate();
    
    // Estados para el cambio de nombre
    const [nombreActual, setNombreActual] = useState('');
    const [nuevoNombre, setNuevoNombre] = useState('');
    const [emailActual, setEmailActual] = useState('');
    const [actualizandoNombre, setActualizandoNombre] = useState(false);

    // Estados para el cambio de contraseña
    const [passwordActual, setPasswordActual] = useState('');
    const [passwordNueva, setPasswordNueva] = useState('');
    const [confirmarPassword, setConfirmarPassword] = useState('');
    const [actualizandoPassword, setActualizandoPassword] = useState(false);

    useEffect(() => {
        const token = localStorage.getItem('token');
        if (!token) {
            navigate('/login');
            return;
        }
        
        try {
            // Mostrar el nombre actual y el email
            const payload = JSON.parse(atob(token.split('.')[1]));
            const nombreDelToken = payload.nombre || '';
            const emailDelToken = payload.email || '';
            setNombreActual(nombreDelToken);
            setNuevoNombre(nombreDelToken);
            setEmailActual(emailDelToken);
        } catch (error) {
            console.error("Error al decodificar token", error);
        }
    }, [navigate]);

    const handleActualizarNombre = async (e) => {
        e.preventDefault();
        setActualizandoNombre(true);
        const token = localStorage.getItem('token');

        try {
            const response = await fetch('https://localhost:7109/api/v1/users/me/profile', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ nuevoNombre }) 
            });

            const data = await response.json();

            if (response.ok) {
                toast.success('¡Nombre actualizado con éxito!');
                setNombreActual(nuevoNombre);
            } else {
                toast.error(data.error || data.detail || 'Error al actualizar el perfil.');
            }
        } catch (error) {
            toast.error('Error de conexión con el servidor.');
        } finally {
            setActualizandoNombre(false);
        }
    };

    const handleCambiarPassword = async (e) => {
        e.preventDefault();
        
        if (passwordNueva !== confirmarPassword) {
            toast.error('Las contraseñas nuevas no coinciden.');
            return;
        }

        setActualizandoPassword(true);
        const token = localStorage.getItem('token');

        try {
            const response = await fetch('https://localhost:7109/api/v1/users/me/password', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ passwordActual, passwordNueva }) 
            });

            if (response.ok) {
                toast.success('¡Contraseña cambiada con éxito!');
                setPasswordActual('');
                setPasswordNueva('');
                setConfirmarPassword('');
            } else {
                const data = await response.json();
                toast.error(data.error || data.detail || 'Error al cambiar la contraseña. Verificá tu clave actual.');
            }
        } catch (error) {
            toast.error('Error de conexión con el servidor.');
        } finally {
            setActualizandoPassword(false);
        }
    };

    return (
        <div className="container mt-5 mb-5">
            <h2 className="mb-4 text-primary"><i className="bi bi-person-gear me-2"></i>Ajustes de Perfil</h2>

            <div className="row g-4">
                {/* Columna izquierda - Cambio de nombre */}
                <div className="col-md-6">
                    <div className="card shadow-sm h-100">
                        <div className="card-header bg-white py-3">
                            <h5 className="mb-0 text-dark"><i className="bi bi-person-badge me-2"></i>Información Personal</h5>
                        </div>
                        <div className="card-body">
                            <form onSubmit={handleActualizarNombre}>
                                <div className="mb-3">
                                    <label className="form-label text-muted small fw-bold">Correo Electrónico</label>
                                    <input 
                                        type="email" 
                                        className="form-control bg-light text-muted" 
                                        value={emailActual}
                                        disabled 
                                    />
                                </div>
                                <div className="mb-3">
                                    <label className="form-label text-muted small fw-bold">Nombre o Apodo Actual</label>
                                    <input 
                                        type="text" 
                                        className="form-control bg-light" 
                                        value={nombreActual}
                                        disabled 
                                    />
                                </div>
                                <div className="mb-4">
                                    <label className="form-label text-muted small fw-bold">Nuevo Nombre</label>
                                    <input 
                                        type="text" 
                                        className="form-control" 
                                        value={nuevoNombre}
                                        onChange={(e) => setNuevoNombre(e.target.value)}
                                        required 
                                        disabled={actualizandoNombre}
                                    />
                                </div>
                                <button 
                                    type="submit" 
                                    className="btn btn-outline-primary w-100 fw-bold"
                                    disabled={actualizandoNombre || nuevoNombre === nombreActual}
                                >
                                    {actualizandoNombre ? 'Guardando...' : 'Actualizar Nombre'}
                                </button>
                            </form>
                        </div>
                    </div>
                </div>

                {/* Columna derecha - Cambio de contraseña */}
                <div className="col-md-6">
                    <div className="card shadow-sm h-100 border-warning">
                        <div className="card-header bg-white py-3 border-warning">
                            <h5 className="mb-0 text-dark"><i className="bi bi-shield-lock me-2 text-warning"></i>Seguridad</h5>
                        </div>
                        <div className="card-body">
                            <form onSubmit={handleCambiarPassword}>
                                <div className="mb-3">
                                    <label className="form-label text-muted small fw-bold">Contraseña Actual</label>
                                    <input 
                                        type="password" 
                                        className="form-control" 
                                        value={passwordActual}
                                        onChange={(e) => setPasswordActual(e.target.value)}
                                        required 
                                        disabled={actualizandoPassword}
                                    />
                                </div>
                                <div className="mb-3">
                                    <label className="form-label text-muted small fw-bold">Nueva Contraseña</label>
                                    <input 
                                        type="password" 
                                        className="form-control" 
                                        placeholder="Mínimo 8 caracteres, 1 mayúscula y 1 número"
                                        value={passwordNueva}
                                        onChange={(e) => setPasswordNueva(e.target.value)}
                                        required 
                                        disabled={actualizandoPassword}
                                    />
                                </div>
                                <div className="mb-4">
                                    <label className="form-label text-muted small fw-bold">Confirmar Nueva Contraseña</label>
                                    <input 
                                        type="password" 
                                        className="form-control" 
                                        value={confirmarPassword}
                                        onChange={(e) => setConfirmarPassword(e.target.value)}
                                        required 
                                        disabled={actualizandoPassword}
                                    />
                                </div>
                                <button 
                                    type="submit" 
                                    className="btn btn-warning w-100 fw-bold"
                                    disabled={actualizandoPassword}
                                >
                                    {actualizandoPassword ? 'Actualizando...' : 'Cambiar Contraseña'}
                                </button>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Perfil;