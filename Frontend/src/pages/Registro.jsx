import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import logo from '../assets/logo.png';

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
            const response = await fetch('https://localhost:7109/api/v1/auth/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ nombre, email, password })
            });

            const data = await response.json();

            if (response.ok) {
                setExito(data.mensaje);
                setTimeout(() => {
                    navigate('/login');
                }, 2500);
            } else {
                setError(data.detail || data.error || 'Ocurrió un error al registrar el usuario.');
            }
        } catch (err) {
            setError('Error de conexión con el servidor.');
        } finally {
            setEnviando(false);
        }
    };

    return (
        <div className="d-flex align-items-center justify-content-center flex-grow-1 w-100 py-5">
            <div className="container">
                <div className="row justify-content-center">
                    <div className="col-md-8 col-lg-5">
                        {/* Borde azul */}
                        <div className="card border-primary shadow-sm rounded-3">
                            <div className="card-body p-4 p-md-5">
                                
                                {/* Logo y Texto con los tamaños ajustados */}
                                <div className="d-flex align-items-center justify-content-center gap-3 mb-5">
                                    <img src={logo} alt="Logo SubastaYa" style={{ height: '80px', width: 'auto', objectFit: 'contain' }} />
                                    <h2 className="text-primary fw-bold mb-0" style={{ fontSize: '2.75rem', letterSpacing: '-1px', transform: 'translateX(-35px)' }}>SubastaYa</h2>
                                </div>
                                
                                {error && <div className="alert alert-danger py-2 small text-center">{error}</div>}
                                {exito && <div className="alert alert-success py-2 small text-center"><i className="bi bi-check-circle me-2"></i>{exito}</div>}
                                
                                <form onSubmit={handleRegistro}>
                                    <div className="mb-3">
                                        <label className="form-label text-muted small mb-1">Nombre o Apodo</label>
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
                                        <label className="form-label text-muted small mb-1">Correo Electrónico</label>
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
                                        <label className="form-label text-muted small mb-1">Contraseña</label>
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
                                        className="btn btn-primary w-100 py-2 mb-3"
                                        disabled={enviando || exito}
                                    >
                                        {enviando ? 'Creando cuenta...' : 'Registrarme'}
                                    </button>
                                </form>
                                
                                <div className="text-center mt-3">
                                    <span className="text-muted small">¿Ya tenés cuenta? </span>
                                    <Link to="/login" className="text-decoration-none fw-bold small">Iniciá sesión</Link>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Registro;