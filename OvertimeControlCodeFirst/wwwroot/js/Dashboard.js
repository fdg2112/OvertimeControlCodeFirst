const formHoras = document.getElementById('formHoras');
const btnCargarHoras = document.getElementById('btnCargarHoras');
let empleadoId = null;

// Mostrar y ocultar el formulario de carga
function cargarFormulario() {
    formHoras.classList.toggle('hidden');
    btnCargarHoras.classList.toggle('hidden');
    if (!formHoras.classList.contains('hidden')) {
        fetch('/Overtime/Create')
            .then(response => response.json())
            .then(data => cargarOpcionesFormulario(data))
            .catch(error => console.error('Error al cargar los datos:', error));
    }
}

// Cargar employees en el formulario
function cargarOpcionesFormulario(data) {
    const empleadoSelect = document.getElementById('empleado');
    empleadoSelect.innerHTML = '<option value="" selected disabled>Seleccione un empleado</option>';
    data.employees.forEach(empleado => {
        empleadoSelect.innerHTML += `
                <option value="${empleado.empleadoId}"
                        data-area-id="${empleado.areaId}"
                        data-area-nombre="${empleado.areaNombre}"
                        data-secretaria-id="${empleado.secretariaId}"
                        data-secretaria-nombre="${empleado.secretariaNombre}">
                    ${empleado.legajo} - ${empleado.nombre} ${empleado.apellido}
                </option>`;
    });
}

// Actualizar área y secretaría en el formulario
function actualizarAreaYSecretaria() {
    const empleadoSelect = document.getElementById('empleado');
    const selectedOption = empleadoSelect.options[empleadoSelect.selectedIndex];
    empleadoId = selectedOption.value;

    const areaId = selectedOption.getAttribute('data-area-id') || '';
    const areaNombre = selectedOption.getAttribute('data-area-nombre') || 'Sin Área';
    const secretariaId = selectedOption.getAttribute('data-secretaria-id') || '';
    const secretariaNombre = selectedOption.getAttribute('data-secretaria-nombre') || 'Sin Secretaría';

    document.getElementById('area').innerHTML = `<option value="${areaId}" selected>${areaNombre}</option>`;
    document.getElementById('secretaria').innerHTML = `<option value="${secretariaId}" selected>${secretariaNombre}</option>`;

    document.getElementById('areaId').value = areaId;
    document.getElementById('secretariaId').value = secretariaId;
}

function toggleForm() {
    formHoras.classList.toggle('hidden');
    btnCargarHoras.classList.toggle('hidden');
}

// Validación y envío del formulario
if (formHoras) {
    formHoras.addEventListener('submit', function (e) {
        e.preventDefault();

        const fechaInicio = new Date(document.getElementById('fechaInicio').value);
        const fechaFin = new Date(document.getElementById('fechaFin').value);

        // Validar fechas
        if (!fechaInicio || !fechaFin) {
            Swal.fire({
                title: 'Error',
                text: 'Debe completar las fechas de inicio y fin.',
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        if (fechaInicio >= fechaFin) {
            Swal.fire({
                title: 'Error',
                text: 'La fecha y hora de inicio deben ser anteriores a la fecha y hora de fin.',
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        const empleadoId = document.getElementById('empleado').value;
        if (!empleadoId) {
            Swal.fire({
                title: 'Error',
                text: 'Debe seleccionar un empleado.',
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        const formData = new FormData(formHoras);

        fetch('/Overtime/Create', {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    Swal.fire({
                        title: 'Éxito',
                        text: data.message,
                        icon: 'success',
                        confirmButtonText: 'OK'
                    }).then(() => location.reload());
                } else {
                    Swal.fire({
                        title: 'Error',
                        text: data.message,
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            })
            .catch(error => {
                console.error('Error:', error);
                Swal.fire({
                    title: 'Error',
                    text: 'Ocurrió un error inesperado.',
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            });
    });
}


// GRAFICOS
document.addEventListener('DOMContentLoaded', () => {
    const { horas50, horas100, gasto50, gasto100, meses, horas50Historico, horas100Historico } = window.dashboardData;

    let horasYGastoChart, horasHistoricasChart;

    // 1. Inicializar gráficos
    function initCharts() {
        const ctx = document.getElementById('horasYGastoChart').getContext('2d');
        horasYGastoChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['50%', '100%'],
                datasets: [
                    {
                        label: 'Horas Realizadas',
                        data: [horas50, horas100],
                        backgroundColor: ['rgba(173, 216, 230, 0.8)'], // Celeste y azul claro
                        yAxisID: 'y-horas',
                    },
                    {
                        label: 'Gasto Mensual',
                        data: [gasto50, gasto100],
                        backgroundColor: ['rgba(135, 206, 250, 0.8)'], // Azul claro y celeste
                        yAxisID: 'y-gasto',
                    }
                ]
            },
            options: {
                responsive: true,
                aspectRatio: 1.75,
                scales: {
                    'y-horas': {
                        type: 'linear',
                        position: 'left',
                        suggestedMax: Math.max(horas50, horas100) * 1.1, // Extiende un 10% por encima del máximo
                        title: {
                            display: true,
                            text: 'Horas',
                        },
                    },
                    'y-gasto': {
                        type: 'linear',
                        position: 'right',
                        suggestedMax: Math.max(gasto50, gasto100) * 1.1, // Extiende un 10% por encima del máximo
                        title: {
                            display: true,
                            text: 'Gasto ($)',
                        },
                        grid: {
                            drawOnChartArea: false,
                        },
                    }
                },
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    datalabels: {
                        anchor: 'end',
                        align: 'top',
                        formatter: (value, ctx) => {
                            if (ctx.dataset.label === 'Gasto Mensual') {
                                return `$${value.toLocaleString('es-AR')}`;
                            }
                            return value;
                        },
                        font: {
                            weight: 'bold',
                        }
                    }
                }
            }
,
            plugins: [ChartDataLabels]
        });

        const horasHistoricasChartCanvas = document.getElementById('horasHistoricasChart').getContext('2d');
        horasHistoricasChart = new Chart(horasHistoricasChartCanvas, {
            type: 'line',
            data: {
                labels: meses,
                datasets: [
                    { label: 'Horas 50%', data: horas50Historico, borderColor: 'rgba(75, 192, 192, 1)', backgroundColor: 'rgba(75, 192, 192, 0.2)', fill: true },
                    { label: 'Horas 100%', data: horas100Historico, borderColor: 'rgba(255, 99, 132, 1)', backgroundColor: 'rgba(255, 99, 132, 0.2)', fill: true }
                ]
            },
            options: { responsive: true, aspectRatio: 1.75 }
        });
    }

    // 3. Actualización de los gráficos
    function updateCharts(data) {
        const horas50 = parseFloat(data.horas50) || 0;
        const horas100 = parseFloat(data.horas100) || 0;
        const gasto50 = parseFloat(data.gasto50) || 0;
        const gasto100 = parseFloat(data.gasto100) || 0;
        const historico50 = data.historico50.map(h => parseFloat(h) || 0);
        const historico100 = data.historico100.map(h => parseFloat(h) || 0);

        // Actualizar gráfico de horas y gasto
        horasYGastoChart.data.datasets[0].data = [horas50, horas100]; // Actualizar las horas
        horasYGastoChart.data.datasets[1].data = [gasto50, gasto100]; // Actualizar el gasto
        horasYGastoChart.update();

        // Actualizar gráfico histórico
        horasHistoricasChart.data.datasets[0].data = historico50;
        horasHistoricasChart.data.datasets[1].data = historico100;
        horasHistoricasChart.update();
    }

    // 4. Manejador del filtro de áreas y secretarías
    function handleFilters() {
        const secretariaFilter = document.getElementById('secretariaFilter');
        const areaFilter = document.getElementById('areaFilter');

        // Actualizar áreas según la secretaría seleccionada
        secretariaFilter?.addEventListener('change', () => {
            const secretariaId = secretariaFilter.value;

            fetch(`/Dashboard/GetAreasBySecretaria?secretariaId=${secretariaId}`)
                .then(response => response.json())
                .then(areas => {
                    areaFilter.innerHTML = '<option value="">Todas las áreas</option>';
                    areas.forEach(area => {
                        areaFilter.innerHTML += `<option value="${area.areaId}">${area.nombreArea}</option>`;
                    });
                    // Actualizar los gráficos si corresponde
                    fetchChartData();
                })
                .catch(error => console.error('Error al cargar las áreas:', error));
        });

        // Filtrar datos según el área seleccionada
        areaFilter?.addEventListener('change', fetchChartData);
    }

    function fetchChartData() {
        const areaId = document.getElementById('areaFilter')?.value || '';
        const secretariaId = document.getElementById('secretariaFilter')?.value || ''; // Secretaría seleccionada (o ninguna)

        if (!areaId && !secretariaId) {
            // Si no hay área ni secretaría seleccionada y el rol es Intendente o Secretario Hacienda
            if (UserRol === "Intendente" || UserRol === "Secretario Hacienda") {
                fetch(`/Dashboard/GetChartData`) // Sin filtros para mostrar todo
                    .then(response => response.json())
                    .then(updateCharts)
                    .catch(error => console.error('Error al cargar los datos del gráfico:', error));
            } else if (UserRol === "Secretario") {
                fetch(`/Dashboard/GetChartData?secretariaId=${UserSecretariaId}`)
                    .then(response => response.json())
                    .then(updateCharts)
                    .catch(error => console.error('Error al cargar los datos del gráfico:', error));
            }
        } else if (areaId) {
            // Si hay un área seleccionada
            fetch(`/Dashboard/GetChartData?areaId=${areaId}`)
                .then(response => response.json())
                .then(updateCharts)
                .catch(error => console.error('Error al cargar los datos del gráfico:', error));
        } else if (secretariaId) {
            // Si hay una secretaría seleccionada
            fetch(`/Dashboard/GetChartData?secretariaId=${secretariaId}`)
                .then(response => response.json())
                .then(updateCharts)
                .catch(error => console.error('Error al cargar los datos del gráfico:', error));
        }
    }


    initCharts();
    handleFilters();
});

document.addEventListener('DOMContentLoaded', () => {
    let donutSecretariaChart, donutAreaChart;

    function initDonutCharts() {
        const ctxSecretariaElement = document.getElementById('donutSecretariaChart');
        const ctxAreaElement = document.getElementById('donutAreaChart');

        if (!ctxSecretariaElement && !ctxAreaElement) {
            console.warn('No se encontraron elementos para los gráficos de dona.');
            return; // Salir si no hay elementos gráficos
        }

        fetch(`/Dashboard/GetDonutChartData`)
            .then(response => response.json())
            .then(data => {
                if (ctxSecretariaElement) {
                    const ctxSecretaria = ctxSecretariaElement.getContext('2d');
                    const secretarias = data.gastoPorSecretaria.map(s => s.secretaria);
                    const gastosSecretarias = data.gastoPorSecretaria.map(s => s.totalGasto);

                    donutSecretariaChart = new Chart(ctxSecretaria, {
                        type: 'doughnut',
                        data: {
                            labels: secretarias,
                            datasets: [{
                                label: 'Gasto por Secretaría',
                                data: gastosSecretarias,
                                backgroundColor: secretarias.map((_, i) => `hsl(${i * 30}, 70%, 70%)`),
                            }]
                        },
                        options: {
                            responsive: true,
                            plugins: {
                                legend: { display: false },
                                tooltip: {
                                    callbacks: {
                                        label: function (context) {
                                            return ` $ ${context.raw.toLocaleString('es-AR')}`;
                                        }
                                    }
                                },
                                datalabels: {
                                    color: 'black',
                                    formatter: function (value, context) {
                                        const label = context.chart.data.labels[context.dataIndex];
                                        const percentage = (value / gastosSecretarias.reduce((a, b) => a + b) * 100).toFixed(1);
                                        return `${label}\n${percentage}%`;
                                    },
                                    font: { size: 12, weight: 'bold' },
                                    align: 'center',
                                    anchor: 'center',
                                }
                            }
                        },
                        plugins: [ChartDataLabels]
                    });
                }

                if (ctxAreaElement) {
                    const ctxArea = ctxAreaElement.getContext('2d');
                    const areas = data.gastoPorArea.map(a => a.area);
                    const gastosAreas = data.gastoPorArea.map(a => a.totalGasto);

                    donutAreaChart = new Chart(ctxArea, {
                        type: 'doughnut',
                        data: {
                            labels: areas,
                            datasets: [{
                                label: 'Gasto por Área',
                                data: gastosAreas,
                                backgroundColor: areas.map((_, i) => `hsl(${i * 30}, 70%, 70%)`),
                            }]
                        },
                        options: {
                            responsive: true,
                            plugins: {
                                legend: { display: false },
                                tooltip: {
                                    callbacks: {
                                        label: function (context) {
                                            return ` $ ${context.raw.toLocaleString('es-AR')}`;
                                        }
                                    }
                                },
                                datalabels: {
                                    color: 'black',
                                    formatter: function (value, context) {
                                        const label = context.chart.data.labels[context.dataIndex];
                                        const percentage = (value / gastosAreas.reduce((a, b) => a + b) * 100).toFixed(1);
                                        return `${label}\n${percentage}%`;
                                    },
                                    font: { size: 12, weight: 'bold' },
                                    align: 'center',
                                    anchor: 'center',
                                }
                            }
                        },
                        plugins: [ChartDataLabels]
                    });
                }
            })
            .catch(error => console.error('Error al cargar los datos de los gráficos tipo dona:', error));
    }


    // Inicializar gráficos al cargar la página
    initDonutCharts();
});


// TABLA DE EMPLEADOS
document.addEventListener('DOMContentLoaded', () => {
    let employeesData = [];
    let currentPage = 1;
    const rowsPerPage = 10;

    // Llamada inicial para cargar employees
    fetchEmpleados();

    // Evento para búsqueda
    document.getElementById('searchInput').addEventListener('keyup', function () {
        const searchValue = this.value.toLowerCase();
        const filteredData = employeesData.filter(emp =>
            `${emp.legajo} ${emp.apellido} ${emp.nombre}`.toLowerCase().includes(searchValue)
        );
        renderTable(filteredData);
    });

    // Función para obtener employees según el área seleccionada
    function fetchEmpleados() {
        const areaId = document.getElementById('areaFilter')?.value || '';

        fetch(`/Dashboard/GetEmpleadosPorArea?areaId=${areaId}`)
            .then(response => response.json())
            .then(data => {
                employeesData = data;
                renderTable(employeesData);
            })
            .catch(error => console.error('Error al cargar employees:', error));
    }

    // Renderizar la tabla con los datos
    function renderTable(data) {
        const tbody = document.querySelector('#employeesTable tbody');
        const start = (currentPage - 1) * rowsPerPage;
        const end = start + rowsPerPage;

        tbody.innerHTML = ''; // Limpiar contenido previo

        // Renderizar solo los employees de la página actual
        const currentData = data.slice(start, end);
        currentData.forEach(emp => {
            const tendencia50 = calcularTendencia(emp.horas50Actual, emp.horas50Anterior);
            const tendencia100 = calcularTendencia(emp.horas100Actual, emp.horas100Anterior);

            tbody.innerHTML += `
            <tr>
                <td>${emp.legajo}</td>
                <td>${emp.apellido}</td>
                <td>${emp.nombre}</td>
                <td>${tendencia50}  ${emp.horas50Actual}</td>
                <td>${tendencia100}  ${emp.horas100Actual}</td>
            </tr>
        `;
        });

        // Actualizar información de paginación
        document.getElementById("pageInfo").innerText = `Página ${currentPage} de ${Math.ceil(data.length / rowsPerPage)}`;
    }

    // Actualizar controles de paginación
    function updatePagination(totalRows) {
        const totalPages = Math.ceil(totalRows / rowsPerPage);
        document.getElementById('pageInfo').textContent = `Página ${currentPage} de ${totalPages}`;
    }

    // Funciones de paginación
    window.prevPage = function () {
        if (currentPage > 1) {
            currentPage--;
            renderTable(employeesData);
        }
    };

    window.nextPage = function () {
        const totalPages = Math.ceil(employeesData.length / rowsPerPage);
        if (currentPage < totalPages) {
            currentPage++;
            renderTable(employeesData);
        }
    };

    // Función para ordenar la tabla
    window.sortTable = function (columnIndex) {
        // Determinar dirección de orden
        const ths = document.querySelectorAll('#employeesTable th');
        const direction = ths[columnIndex].dataset.direction === 'asc' ? 'desc' : 'asc';

        // Resetear direcciones de todos los encabezados
        ths.forEach(th => th.removeAttribute('data-direction'));
        ths[columnIndex].setAttribute('data-direction', direction);

        // Ordenar el dataset global completo (employeesData)
        employeesData.sort((a, b) => {
            const aValue = Object.values(a)[columnIndex];
            const bValue = Object.values(b)[columnIndex];

            if (typeof aValue === 'string' && typeof bValue === 'string') {
                // Ordenar cadenas (ignorando mayúsculas)
                return direction === 'asc'
                    ? aValue.localeCompare(bValue)
                    : bValue.localeCompare(aValue);
            }

            // Ordenar números o valores no cadenas
            return direction === 'asc' ? aValue - bValue : bValue - aValue;
        });

        // Renderizar nuevamente la tabla paginada después de ordenar
        renderTable(employeesData);
    };


    // Función para calcular tendencia
    function calcularTendencia(actual, anterior) {
        if (actual > anterior) {
            return '<span style="color: red;">▲</span>';
        } else if (actual < anterior) {
            return '<span style="color: green;">▼</span>';
        } else {
            return '<span style="color: black;">–</span>';
        }
    }
});
