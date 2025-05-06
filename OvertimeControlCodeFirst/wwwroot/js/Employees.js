// Tabla de employees
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
            `${emp.legajo} ${emp.apellido} ${emp.nombre} ${emp.categoriaNombre} ${emp.areaNombre} ${emp.secretariaNombre}`.toLowerCase().includes(searchValue)
        );
        renderTable(filteredData);
    });

    // Función para obtener employees según el área seleccionada
    function fetchEmpleados() {
        const areaId = document.getElementById('areaFilter')?.value || '';

        fetch(`/Employee/GetEmpleados?areaId=${areaId}`)
            .then(response => response.json())
            .then(data => {
                console.log(data);
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
            tbody.innerHTML += `
            <tr>
                <td>${emp.legajo}</td>
                <td>${emp.apellido}</td>
                <td>${emp.nombre}</td>
                <td>${emp.categoriaNombre}</td>
                <td>${emp.areaNombre}</td>
                <td>${emp.secretariaNombre}</td>
                <td>
                    <button class="btn btn-warning btn-sm" onclick="editarEmpleado(${emp.empleadoId})">Editar</button>
                </td>
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
        // Mapeo de índices de columnas a las propiedades de employeesData
        const columnMap = {
            0: 'legajo',
            1: 'apellido',
            2: 'nombre',
            3: 'categoriaNombre',
            4: 'areaNombre',
            5: 'secretariaNombre'
        };

        const sortKey = columnMap[columnIndex];
        if (!sortKey) return; // Salir si el índice no es válido

        // Determinar la dirección de orden
        const ths = document.querySelectorAll('#employeesTable th');
        const direction = ths[columnIndex].dataset.direction === 'asc' ? 'desc' : 'asc';

        // Resetear direcciones de todos los encabezados
        ths.forEach(th => th.removeAttribute('data-direction'));
        ths[columnIndex].setAttribute('data-direction', direction);

        // Ordenar employeesData según la clave correspondiente
        employeesData.sort((a, b) => {
            const aValue = a[sortKey] || ''; // Manejar valores nulos o indefinidos
            const bValue = b[sortKey] || '';

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

});

// Formulario de Carga de Empleados
const formEmpleado = document.getElementById('formEmpleado');
const btnAgregarEmpleado = document.getElementById('btnAgregarEmpleado');

if (formEmpleado) {
    formEmpleado.addEventListener('submit', function (e) {
        e.preventDefault();
        const formData = new FormData(formEmpleado);
        fetch('/Employee/CreateEmployee', {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! Status: ${response.status}`);
                }
                return response.json();
            })
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
                console.error('Error en la solicitud:', error);
                Swal.fire({
                    title: 'Error',
                    text: 'Ocurrió un error inesperado.',
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            });
    });
    document.getElementById("legajo").addEventListener("blur", function () {
        const legajoInput = this.value;

        // Validar que sea un número de 3 dígitos
        if (!/^\d{3}$/.test(legajoInput)) {
            Swal.fire({
                title: "Error",
                text: "El legajo debe ser un número de 3 dígitos.",
                icon: "error",
                confirmButtonText: "OK"
            });
            this.value = ""; // Limpiar el campo
            return;
        }

        // Verificar si el legajo ya existe en la base de datos
        fetch(`/Employee/CheckLegajo?legajo=${legajoInput}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! Status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                if (data.exists) {
                    Swal.fire({
                        title: "Legajo ya registrado",
                        html: `
                        <p>El legajo ingresado pertenece a:</p>
                        <p><strong>${data.empleado.nombre} ${data.empleado.apellido}</strong></p>
                        <p>Área: ${data.empleado.areaNombre}</p>
                        <p>Secretaría: ${data.empleado.secretariaNombre}</p>
                        <p>Para agregar a este empleado tu área, debes solicitar la transferencia a su superior.</p>
                    `,
                        icon: "warning",
                        confirmButtonText: "OK"
                    });
                    document.getElementById("legajo").value = ""; // Limpiar el campo
                }
            })
            .catch(error => console.error("Error al verificar el legajo:", error));
    });
}
function cargarFormulario() {
    formEmpleado.classList.toggle("hidden");
    btnAgregarEmpleado.classList.toggle("hidden");

    if (!formEmpleado.classList.contains("hidden")) {
        // Solicitar datos filtrados según el usuario logueado
        fetch("/Employee/GetAreasAndSecretarias")
            .then((response) => response.json())
            .then((data) => {
                cargarOpcionesConSeleccion(data);
            })
            .catch((error) => console.error("Error al cargar las opciones:", error));
    }
}
function cargarOpcionesConSeleccion(data) {
    // Cargar áreas
    const areaSelect = document.getElementById("areaId");
    areaSelect.innerHTML = '<option value="" disabled>Seleccione un área</option>';
    data.areas.forEach((area) => {
        const selected = data.defaultAreaId === area.id ? "selected" : "";
        areaSelect.innerHTML += `<option value="${area.id}" ${selected}>${area.nombre}</option>`;
    });

    // Cargar secretarías
    const secretariaSelect = document.getElementById("secretariaId");
    secretariaSelect.innerHTML = '<option value="" disabled>Seleccione una secretaría</option>';
    data.secretarias.forEach((secretaria) => {
        const selected = data.defaultSecretariaId === secretaria.id ? "selected" : "";
        secretariaSelect.innerHTML += `<option value="${secretaria.id}" ${selected}>${secretaria.nombre}</option>`;
    });

    // Cargar categorias
    const categoriaSelect = document.getElementById("categoriaId");
    categoriaSelect.innerHTML = '<option value="" disabled>Seleccione una categoria</option>';
    data.categorias.forEach((categoria) => {
        const selected = data.defaultCategoriaId === categoria.id ? "selected" : "";
        categoriaSelect.innerHTML += `<option value="${categoria.id}" ${selected}>${categoria.nombre}</option>`;
    });


}
function toggleForm() {
    formEmpleado.classList.toggle('hidden');
    btnAgregarEmpleado.classList.toggle('hidden');
}

// Editar datos de un empleado
function editarEmpleado(empleadoId) {
    // Obtener los datos del empleado desde el servidor
    fetch(`/Employee/GetEmpleadoById?id=${empleadoId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            // Generar formulario pre-cargado
            const formHtml = `
                <form id="formEditarEmpleado">
                    <div class="mb-3">
                        <label for="nombre" class="form-label">Nombre</label>
                        <input type="text" class="form-control" id="nombre" name="Nombre" value="${data.nombre}" required>
                    </div>
                    <div class="mb-3">
                        <label for="apellido" class="form-label">Apellido</label>
                        <input type="text" class="form-control" id="apellido" name="Apellido" value="${data.apellido}" required>
                    </div>
                    <div class="mb-3">
                        <label for="categoriaId" class="form-label">Categoría Salarial</label>
                        <select class="form-select" id="categoriaId" name="CategoriaId" required>
                            ${data.categorias.map(c => `
                                <option value="${c.categoriaId}" ${c.categoriaId === data.categoriaId ? "selected" : ""}>
                                    ${c.nombreCategoria}
                                </option>`).join("")}
                        </select>
                    </div>
                    <div class="mb-3">
                        <label for="areaId" class="form-label">Área</label>
                        <select class="form-select" id="areaId" name="AreaId" required>
                            ${data.areas.map(a => `
                                <option value="${a.areaId}" ${a.areaId === data.areaId ? "selected" : ""}>
                                    ${a.nombreArea}
                                </option>`).join("")}
                        </select>
                    </div>
                    <div class="mb-3">
                        <label for="secretariaId" class="form-label">Secretaría</label>
                        <select class="form-select" id="secretariaId" name="SecretariaId" required>
                            ${data.secretarias.map(s => `
                                <option value="${s.secretariaId}" ${s.secretariaId === data.secretariaId ? "selected" : ""}>
                                    ${s.nombreSecretaria}
                                </option>`).join("")}
                        </select>
                    </div>
                </form>
            `;

            // Mostrar SweetAlert con el formulario cargado
            Swal.fire({
                title: "Editar Empleado",
                html: formHtml,
                showCancelButton: true,
                confirmButtonText: "Guardar",
                preConfirm: () => {
                    // Obtener datos del formulario
                    const form = document.getElementById("formEditarEmpleado");
                    const formData = new FormData(form);

                    // Enviar los datos actualizados al servidor
                    return fetch(`/Employee/EditEmpleado?id=${empleadoId}`, {
                        method: "POST",
                        body: formData,
                        headers: {
                            "X-Requested-With": "XMLHttpRequest",
                            "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value
                        }
                    })
                        .then(response => {
                            if (!response.ok) {
                                throw new Error("Error al actualizar el empleado.");
                            }
                            return response.json();
                        })
                        .catch(error => {
                            Swal.showValidationMessage(`Error: ${error.message}`);
                        });
                }
            }).then(result => {
                if (result.isConfirmed) {
                    Swal.fire({
                        title: "Éxito",
                        text: "Empleado actualizado correctamente.",
                        icon: "success",
                        confirmButtonText: "OK"
                    }).then(() => location.reload());
                }
            });
        })
        .catch(error => {
            Swal.fire({
                title: "Error",
                text: `No se pudo cargar el empleado: ${error.message}`,
                icon: "error",
                confirmButtonText: "OK"
            });
        });
}

