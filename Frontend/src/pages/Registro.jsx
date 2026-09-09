import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';

const Registro = () => {
    const [nombre, setNombre] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    
    const [error, setError] = useState('');
    const [exito, setExito] = useState('');
    const [enviando, setEnviando] = useState(false);
    
    const navigate = useNavigate();

    const handleRegistro = async (e) => {
        e.preventDefault();
        setError('');
        setExito('');
        setEnviando(true);

        try {
            // Llamamos al endpoint que ya tienen armado en C#
            const response = await fetch('https://localhost:7109/api/v1/auth/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ nombre, email, password })
            });

            const data = await response.json();

            if (response.ok) {
                // Mostramos el mensaje de éxito del backend ("Usuario registrado exitosamente...")
                setExito(data.mensaje);
                
                // Esperamos 2.5 segundos y lo redirigimos automáticamente al login
                setTimeout(() => {
                    navigate('/login');
                }, 2500);
            } else {
                // Atrapamos los errores de validación (ej. claves débiles o email duplicado)
                // Usamos data.detail porque tu ExceptionMiddleware usa ese formato RFC 7807
                setError(data.detail || data.error || 'Ocurrió un error al registrar el usuario.');
            }
        } catch (err) {
            setError('Error de conexión con el servidor.');
        } finally {
            setEnviando(false);
        }
    };

    return (
        <div className="container mt-5">
            <div className="row justify-content-center">
                <div className="col-md-6 col-lg-5">
                    <div className="card shadow-sm border-primary">
                        <div className="card-body p-4">
                            <h2 className="text-center mb-4 text-primary">Crear Cuenta</h2>
                            
                            {error && <div className="alert alert-danger small">{error}</div>}
                            {exito && <div className="alert alert-success small"><i className="bi bi-check-circle me-2"></i>{exito}</div>}
                            
                            <form onSubmit={handleRegistro}>
                                <div className="mb-3">
                                    <label className="form-label">Nombre o Apodo</label>
                                    <input 
                                        type="text" 
                                        className="form-control" 
                                        value={nombre}
                                        onChange={(e) => setNombre(e.target.value)}
                                        required 
                                        disabled={enviando || exito}
                                    />
                                </div>
                                <div className="mb-3">
                                    <label className="form-label">Correo Electrónico</label>
                                    <input 
                                        type="email" 
                                        className="form-control" 
                                        value={email}
                                        onChange={(e) => setEmail(e.target.value)}
                                        required 
                                        disabled={enviando || exito}
                                    />
                                </div>
                                <div className="mb-4">
                                    <label className="form-label">Contraseña</label>
                                    <input 
                                        type="password" 
                                        className="form-control" 
                                        value={password}
                                        onChange={(e) => setPassword(e.target.value)}
                                        required 
                                        disabled={enviando || exito}
                                        placeholder="Mínimo 8 caracteres, 1 mayúscula y 1 número"
                                    />
                                </div>
                                <button 
                                    type="submit" 
                                    className="btn btn-primary w-100 mb-3"
                                    disabled={enviando || exito}
                                >
                                    {enviando ? 'Creando cuenta...' : 'Registrarme'}
                                </button>
                            </form>
                            
                            <div className="text-center mt-3">
                                <span className="text-muted small">¿Ya tenés cuenta? </span>
                                <Link to="/login" className="text-decoration-none fw-bold">Iniciá sesión</Link>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Registro;