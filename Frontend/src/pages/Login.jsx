import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';

const Login = () => {
    const [correo, setCorreo] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleLogin = async (e) => {
        e.preventDefault();
        setError('');

        try {
            // Hacemos el POST al endpoint del backend
            const response = await fetch('https://localhost:7109/api/v1/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ correo, password })
            });

            const data = await response.json();

            if (response.ok) {
                // Guardamos el token en el almacenamiento del navegador
                localStorage.setItem('token', data.token);
                // Redirigimos al catálogo
                navigate('/catalogo');
            } else {
                // Si la contraseña o correo están mal, mostramos el error de la API
                setError(data.error || 'Ocurrió un error al iniciar sesión.');
            }
        } catch (err) {
            setError('Error de conexión con el servidor.');
        }
    };

    return (
        <div className="container mt-5">
            <div className="row justify-content-center">
                <div className="col-md-6 col-lg-4">
                    <div className="card shadow-sm">
                        <div className="card-body p-4">
                            <h2 className="text-center mb-4 text-primary">SubastaYa</h2>
                            
                            {error && <div className="alert alert-danger">{error}</div>}
                            
                            <form onSubmit={handleLogin}>
                                <div className="mb-3">
                                    <label className="form-label">Correo Electrónico</label>
                                    <input 
                                        type="email" 
                                        className="form-control" 
                                        value={correo}
                                        onChange={(e) => setCorreo(e.target.value)}
                                        required 
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
                                    />
                                </div>
                                <button type="submit" className="btn btn-primary w-100">
                                    Iniciar Sesión
                                </button>
                            </form>
                            
                            <div className="text-center mt-3">
                                <Link to="/registro" className="text-decoration-none">¿No tienes cuenta? Regístrate</Link>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Login;