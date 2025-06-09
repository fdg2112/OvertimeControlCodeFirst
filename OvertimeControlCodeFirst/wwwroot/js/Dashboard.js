const formHours = document.getElementById('formHours');
const btnUploadHours = document.getElementById('btnUploadHours');
let employeeId = null;

// Mostrar y ocultar el formulario de carga
function loadForm() {
    formHours.classList.toggle('hidden');
    btnUploadHours.classList.toggle('hidden');
    if (!formHours.classList.contains('hidden')) {
        fetch('/Overtime/Create')
            .then(response => response.json())
            .then(data => loadFormOptions(data))
            .catch(error => console.error('Error al cargar los datos:', error));
    }
}

// Cargar employees en el formulario
function loadFormOptions(data) {
    const employeeSelect = document.getElementById('employee');
    employeeSelect.innerHTML = '<option value="" selected disabled>Seleccione un empleado</option>';
    data.employees.forEach(emp => {
        const option = document.createElement('option');
        option.value = emp.employeeId;
        option.textContent = `${emp.recordNumber} - ${emp.lastName} ${emp.name}`;
        option.setAttribute('data-area-id', emp.areaId);
        option.setAttribute('data-area-name', emp.areaNombre);
        option.setAttribute('data-secretariat-id', emp.secretariatId);
        option.setAttribute('data-secretariat-name', emp.secretariaNombre);
        employeeSelect.appendChild(option);
    });
}

// Actualizar área y secretaría en el formulario
function updateAreaAndSecretariat() {
    const employeeSelect = document.getElementById('employee');
    const selectedOption = employeeSelect.options[employeeSelect.selectedIndex];
    employeeId = selectedOption.value;

    const areaId = selectedOption.getAttribute('data-area-id') || '';
    const areaName = selectedOption.getAttribute('data-area-name') || 'Sin Área';
    const secretariatId = selectedOption.getAttribute('data-secretariat-id') || '';
    const secretariatName = selectedOption.getAttribute('data-secretariat-name') || 'Sin Secretaría';

    document.getElementById('area').innerHTML = `<option value="${areaId}" selected>${areaName}</option>`;
    document.getElementById('secretariat').innerHTML = `<option value="${secretariatId}" selected>${secretariatName}</option>`;

    document.getElementById('areaId').value = areaId;
    document.getElementById('secretariatId').value = secretariatId;
}

function toggleForm() {
    formHours.classList.toggle('hidden');
    btnUploadHours.classList.toggle('hidden');
}

// Validación y envío del formulario
if (formHours) {
    formHours.addEventListener('submit', function (e) {
        e.preventDefault();

        const startDate = new Date(document.getElementById('startDate').value);
        const endDate = new Date(document.getElementById('endDate').value);

        // Validar fechas
        if (!startDate || !endDate) {
            Swal.fire({
                title: 'Error',
                text: 'Debe completar las fechas de inicio y fin.',
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        if (startDate >= endDate) {
            Swal.fire({
                title: 'Error',
                text: 'La fecha y hora de inicio deben ser anteriores a la fecha y hora de fin.',
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        const employeeId = document.getElementById('employee').value;
        if (!employeeId) {
            Swal.fire({
                title: 'Error',
                text: 'Debe seleccionar un empleado.',
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        const formData = new FormData(formHours);

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
    const { overtimes50, overtimes100, expense50, expense100, months, historicalOvertimes50, historicalOvertimes100 } = window.dashboardData;
    let overtimesAndCostChart, historicOvertimesChart;

    function initCharts() {
        console.log("UserRole:", UserRole);
        console.log("UserSecretariatId:", UserSecretariatId);
        console.log("UserAreaId:", UserAreaId);
        const ctx = document.getElementById('overtimesAndCostChart').getContext('2d');
        overtimesAndCostChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['50%', '100%'],
                datasets: [
                    {
                        label: 'Horas Realizadas',
                        data: [overtimes50, overtimes100],
                        backgroundColor: ['rgba(173, 216, 230, 0.8)'], // Celeste y azul claro
                        yAxisID: 'y-horas',
                    },
                    {
                        label: 'Gasto Mensual',
                        data: [expense50, expense100],
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
                        suggestedMax: Math.max(overtimes50, overtimes100) * 1.1, // Extiende un 10% por encima del máximo
                        title: {
                            display: true,
                            text: 'Horas',
                        },
                    },
                    'y-gasto': {
                        type: 'linear',
                        position: 'right',
                        suggestedMax: Math.max(expense50, expense100) * 1.1, // Extiende un 10% por encima del máximo
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
                                return value !== undefined && value !== null
                                    ? `$${value.toLocaleString('es-AR')}`
                                    : '';
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

        const horasHistoricasChartCanvas = document.getElementById('historicOvertimesChart').getContext('2d');
        historicOvertimesChart = new Chart(horasHistoricasChartCanvas, {
            type: 'line',
            data: {
                labels: months,
                datasets: [
                    { label: 'Horas 50%', data: historicalOvertimes50, borderColor: 'rgba(75, 192, 192, 1)', backgroundColor: 'rgba(75, 192, 192, 0.2)', fill: true },
                    { label: 'Horas 100%', data: historicalOvertimes100, borderColor: 'rgba(255, 99, 132, 1)', backgroundColor: 'rgba(255, 99, 132, 0.2)', fill: true }
                ]
            },
            options: { responsive: true, aspectRatio: 1.75 }
        });
    }

    function updateCharts(data) {
        const overtimes50 = parseFloat(data.overtimes50) || 0;
        const overtimes100 = parseFloat(data.overtimes100) || 0;
        const expense50 = parseFloat(data.expense50) || 0;
        const expense100 = parseFloat(data.expense100) || 0;
        const historical50 = data.historical50.map(h => parseFloat(h) || 0);
        const historical100 = data.historical100.map(h => parseFloat(h) || 0);

        overtimesAndCostChart.data.datasets[0].data = [overtimes50, overtimes100]; // Actualizar las horas
        overtimesAndCostChart.data.datasets[1].data = [expense50, expense100]; // Actualizar el gasto
        overtimesAndCostChart.update();
        historicOvertimesChart.data.datasets[0].data = historical50;
        historicOvertimesChart.data.datasets[1].data = historical100;
        historicOvertimesChart.update();
    }

    function handleFilters() {
        const secretariatFilter = document.getElementById('secretariatFilter');
        const areaFilter = document.getElementById('areaFilter');

        secretariatFilter?.addEventListener('change', () => {
            const secretariatId = secretariatFilter.value;
            fetch(`/Dashboard/GetAreasBySecretariat?secretariatId=${secretariatId}`)
                .then(response => response.json())
                .then(areas => {
                    areaFilter.innerHTML = '<option value="">Todas las áreas</option>';
                    areas.forEach(area => {
                        areaFilter.innerHTML += `<option value="${area.areaId}">${area.name}</option>`;
                    });
                    fetchChartData();
                })
                .catch(error => console.error('Error al cargar las áreas:', error));
        });
        areaFilter?.addEventListener('change', fetchChartData);
    }

    function fetchChartData() {
        console.log("UserRole:", UserRole);
        console.log("UserSecretariatId:", UserSecretariatId);

        const areaId = document.getElementById('areaFilter')?.value || '';
        const secretariatId = document.getElementById('secretariatFilter')?.value || ''; // Secretaría seleccionada (o ninguna)

        if (!areaId && !secretariatId) {
            if (UserRole === "Intendente" || UserRole === "Secretario Hacienda") {
                fetch(`/Dashboard/GetChartData`)
                    .then(response => response.json())
                    .then(updateCharts)
                    .catch(error => console.error('Error al cargar los datos del gráfico:', error));
            } else if (UserRole === "Secretario") {
                fetch(`/Dashboard/GetChartData?secretariatId=${UserSecretariatId}`)
                    .then(response => response.json())
                    .then(updateCharts)
                    .catch(error => console.error('Error al cargar los datos del gráfico:', error));
            } else if (UserRole === "Jefe de Área") {
                fetch(`/Dashboard/GetChartData?areaId=${UserAreaId}`)
                    .then(response => response.json())
                    .then(updateCharts)
                    .catch(error => console.error('Error al cargar los datos del gráfico:', error));
            }
        } else if (areaId) {
            fetch(`/Dashboard/GetChartData?areaId=${areaId}`)
                .then(response => response.json())
                .then(updateCharts)
                .catch(error => console.error('Error al cargar los datos del gráfico:', error));
        } else if (secretariatId) {
            fetch(`/Dashboard/GetChartData?secretariatId=${secretariatId}`)
                .then(response => response.json())
                .then(updateCharts)
                .catch(error => console.error('Error al cargar los datos del gráfico:', error));
        }
    }

    initCharts();
    handleFilters();
    fetchChartData();
});

document.addEventListener('DOMContentLoaded', () => {
    let donutSecretariatChart, donutAreaChart;

    function initDonutCharts() {
        const ctxSecretariatElement = document.getElementById('donutSecretariatChart');
        const ctxAreaElement = document.getElementById('donutAreaChart');

        if (!ctxSecretariatElement && !ctxAreaElement) {
            console.warn('No se encontraron elementos para los gráficos de dona.');
            return;
        }

        fetch(`/Dashboard/GetDonutChartData`)
            .then(response => response.json())
            .then(data => {
                if (ctxSecretariatElement) {
                    const ctxSecretariat = ctxSecretariatElement.getContext('2d');
                    const secretariats = data.expenseBySecretariat.map(s => s.secretariat);
                    const secretariatExpenses = data.expenseBySecretariat.map(s => s.totalExpense);

                    donutSecretariatChart = new Chart(ctxSecretariat, {
                        type: 'doughnut',
                        data: {
                            labels: secretariats,
                            datasets: [{
                                label: 'Gasto por Secretaría',
                                data: secretariatExpenses,
                                backgroundColor: secretariats.map((_, i) => `hsl(${i * 30}, 70%, 70%)`),
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
                                        const percentage = (value / secretariatExpenses.reduce((a, b) => a + b) * 100).toFixed(1);
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
                    const areas = data.expenseByArea.map(a => a.area);
                    const areasExpenses = data.expenseByArea.map(a => a.totalExpense);

                    donutAreaChart = new Chart(ctxArea, {
                        type: 'doughnut',
                        data: {
                            labels: areas,
                            datasets: [{
                                label: 'Gasto por Área',
                                data: areasExpenses,
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
                                        const percentage = (value / areasExpenses.reduce((a, b) => a + b) * 100).toFixed(1);
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

    fetchEmployees();

    document.getElementById('searchInput').addEventListener('keyup', function () {
        const searchValue = this.value.toLowerCase();
        const filteredData = employeesData.filter(emp =>
            `${emp.recordNumber} ${emp.lastName} ${emp.name}`.toLowerCase().includes(searchValue)
        );
        renderTable(filteredData);
    });

    function fetchEmployees() {
        const areaId = document.getElementById('areaFilter')?.value || '';

        fetch(`/Dashboard/GetEmployeesByArea?areaId=${areaId}`)
            .then(response => response.json())
            .then(data => {
                employeesData = data;
                renderTable(employeesData);
            })
            .catch(error => console.error('Error al cargar empelados:', error));
    }

    function renderTable(data) {
        const tbody = document.querySelector('#employeesTable tbody');
        const start = (currentPage - 1) * rowsPerPage;
        const end = start + rowsPerPage;

        tbody.innerHTML = '';

        const currentData = data.slice(start, end);
        currentData.forEach(emp => {
            const tendency50 = calculateTrend(emp.currentOvertimes50, emp.previousOvertimes50);
            const tendency100 = calculateTrend(emp.currentOvertimes100, emp.previousOvertimes100);

            tbody.innerHTML += `
            <tr>
                <td>${emp.recordNumber}</td>
                <td>${emp.lastName}</td>
                <td>${emp.name}</td>
                <td>${tendency50}  ${emp.currentOvertimes50}</td>
                <td>${tendency100}  ${emp.currentOvertimes100}</td>
            </tr>
        `;
        });
        document.getElementById("pageInfo").innerText = `Página ${currentPage} de ${Math.ceil(data.length / rowsPerPage)}`;
    }
    function updatePagination(totalRows) {
        const totalPages = Math.ceil(totalRows / rowsPerPage);
        document.getElementById('pageInfo').textContent = `Página ${currentPage} de ${totalPages}`;
    }

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

    window.sortTable = function (columnIndex) {
        const ths = document.querySelectorAll('#employeesTable th');
        const direction = ths[columnIndex].dataset.direction === 'asc' ? 'desc' : 'asc';
        ths.forEach(th => th.removeAttribute('data-direction'));
        ths[columnIndex].setAttribute('data-direction', direction);
        employeesData.sort((a, b) => {
            const aValue = Object.values(a)[columnIndex];
            const bValue = Object.values(b)[columnIndex];
            if (typeof aValue === 'string' && typeof bValue === 'string') {
                return direction === 'asc'
                    ? aValue.localeCompare(bValue)
                    : bValue.localeCompare(aValue);
            }
            return direction === 'asc' ? aValue - bValue : bValue - aValue;
        });
        renderTable(employeesData);
    };

    function calculateTrend(current, previous) {
        if (current > previous) {
            return '<span style="color: red;">▲</span>';
        } else if (current < previous) {
            return '<span style="color: green;">▼</span>';
        } else {
            return '<span style="color: black;">–</span>';
        }
    }
});
