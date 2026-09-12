import { useState, useEffect } from 'react';

const ContadorRegresivo = ({ fechaInicio, fechaFin, estado, enDetalle = false }) => {
    const esProgramada = estado === 'PROGRAMADA';
    const fechaObjetivo = esProgramada ? fechaInicio : fechaFin;
    const etiqueta = esProgramada ? 'Comienza en:' : 'Tiempo restante:';

    const calcularTiempoRestante = () => {
        if (!fechaObjetivo) return null; 
        
        const fechaString = fechaObjetivo.endsWith('Z') ? fechaObjetivo : `${fechaObjetivo}Z`;
        const fecha = new Date(fechaString);
        
        if (isNaN(fecha.getTime())) return null; 

        const diferencia = fecha - new Date();
        if (diferencia <= 0) return null;

        return {
            totalMilisegundos: diferencia,
            dias: Math.floor(diferencia / (1000 * 60 * 60 * 24)),
            horas: Math.floor((diferencia / (1000 * 60 * 60)) % 24).toString().padStart(2, '0'),
            minutos: Math.floor((diferencia / 1000 / 60) % 60).toString().padStart(2, '0'),
            segundos: Math.floor((diferencia / 1000) % 60).toString().padStart(2, '0')
        };
    };

    const [tiempo, setTiempo] = useState(calcularTiempoRestante());

    useEffect(() => {
        if (estado !== 'ACTIVA' && estado !== 'PROGRAMADA') return;

        const timer = setInterval(() => {
            const nuevoTiempo = calcularTiempoRestante();
            setTiempo(nuevoTiempo);
            if (!nuevoTiempo) clearInterval(timer);
        }, 1000);

        return () => clearInterval(timer);
    }, [fechaObjetivo, estado]);

    if (estado === 'FINALIZADA' || estado === 'CANCELADA' || estado === 'DESIERTA') {
        return <span className="text-muted d-block mt-1 fw-bold">Subasta {estado.toLowerCase()}</span>;
    }

    // LÓGICA DE ZONA CRÍTICA: Menos de 1 minuto restante y está activa
    const esCritico = tiempo && tiempo.totalMilisegundos < 60000 && !esProgramada;

    return (
        <div className={`text-center ${esCritico && enDetalle ? 'bg-danger text-white p-2 rounded shadow-sm' : ''}`}>
            <small className={`d-block mb-1 ${esCritico && enDetalle ? 'text-white' : 'text-muted'}`}>{etiqueta}</small>
            {!tiempo ? (
                <span className={`fw-bold ${esProgramada ? 'text-success' : 'text-danger'}`}>
                    {esProgramada ? '¡Por comenzar!' : '¡Tiempo agotado!'}
                </span>
            ) : (
                <span className={`fw-bold ${esCritico && enDetalle ? 'text-white fs-5' : esProgramada ? 'text-primary' : 'text-danger'}`}>
                    <i className="bi bi-clock me-1"></i>
                    {tiempo.dias > 0 && `${tiempo.dias}d `}
                    {tiempo.horas}:{tiempo.minutos}:{tiempo.segundos}
                </span>
            )}
        </div>
    );
};

export default ContadorRegresivo;