export const formatearFechaLocal = (fechaString) => {
    if (!fechaString) return '';
    
    // Le agregamos la 'Z' para que JS sepa que viene en UTC
    const utcString = fechaString.endsWith('Z') ? fechaString : `${fechaString}Z`;
    const fecha = new Date(utcString);
    
    return fecha.toLocaleString('es-AR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
};