import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import logo from '../assets/logo.png';

const Login = () => {
    const [correo, setCorreo] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleLogin = async (e) => {
        e.preventDefault();
        setError('');

        try {
            const response = await fetch('https://localhost:7109/api/v1/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ correo, password })
            });

            const data = await response.json();

            if (response.ok) {
                localStorage.setItem('token', data.token);
                navigate('/catalogo');
            } else {
                setError(data.error || 'Ocurrió un error al iniciar sesión.');
            }
        } catch (err) {
            setError('Error de conexión con el servidor.');
        }
    };

    return (
        // Centrado para que ocupe toda la pantalla
        <div className="d-flex align-items-center justify-content-center flex-grow-1 w-100 py-5">
            <div className="container">
                <div className="row justify-content-center">
                    {/* Tarjeta un poco más ancha en desktop */}
                    <div className="col-md-8 col-lg-5">
                        <div className="card border-primary shadow-sm rounded-3">
                            {/* Padding interno*/}
                            <div className="card-body p-4 p-md-5">
                                
                                {/* Logo y Texto alineados */}
                                <div className="d-flex align-items-center justify-content-center gap-3 mb-5">
                                    <img src={logo} alt="Logo SubastaYa" style={{ height: '80px', width: 'auto', objectFit: 'contain' }} />
                                    <h2 className="text-primary fw-bold mb-0" style={{ fontSize: '2.75rem', letterSpacing: '-1px', transform: 'translateX(-35px)' }}>SubastaYa</h2>
                                </div>
                                
                                {error && <div className="alert alert-danger py-2 small text-center">{error}</div>}
                                
                                <form onSubmit={handleLogin}>
                                    <div className="mb-3">
                                        <label className="form-label text-muted small mb-1">Correo Electrónico</label>
                                        <input 
                                            type="email" 
                                            className="form-control" 
                                            value={correo}
                                            onChange={(e) => setCorreo(e.target.value)}
                                            required 
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
                                        />
                                    </div>
                                    <button type="submit" className="btn btn-primary w-100 py-2">
                                        Iniciar Sesión
                                    </button>
                                </form>
                                
                                <div className="text-center mt-4">
                                    <Link to="/registro" className="text-decoration-none small">
                                        ¿No tienes cuenta? Regístrate
                                    </Link>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Login;