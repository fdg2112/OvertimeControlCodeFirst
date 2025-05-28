document.addEventListener('DOMContentLoaded', () => {
    let employeesData = [];
    let currentPage = 1;
    const rowsPerPage = 10;

    fetchEmployees();

    document.getElementById('searchInput').addEventListener('keyup', function () {
        const searchValue = this.value.toLowerCase();
        const filteredData = employeesData.filter(emp =>
            `${emp.recordNumber} ${emp.lastName} ${emp.name} ${emp.categoryNumber} ${emp.areaName} ${emp.secretariatName}`.toLowerCase().includes(searchValue)
        );
        renderTable(filteredData);
    });

    function fetchEmployees() {
        const areaId = document.getElementById('areaFilter')?.value || '';

        // CORRECCIÓN: Cambiar GetEmpleados por GetEmployy
        fetch(`/Employee/GetEmployy?areaId=${areaId}`)
            .then(response => response.json())
            .then(data => {
                console.log(data);
                employeesData = data;
                renderTable(employeesData);
            })
            .catch(error => console.error('Error al cargar empleados:', error));
    }

    function renderTable(data) {
        const tbody = document.querySelector('#employeesTable tbody');
        const start = (currentPage - 1) * rowsPerPage;
        const end = start + rowsPerPage;

        tbody.innerHTML = ''; // Limpiar contenido previo

        const currentData = data.slice(start, end);
        currentData.forEach(emp => {
            tbody.innerHTML += `
            <tr>
                <td>${emp.recordNumber}</td>
                <td>${emp.lastName}</td>
                <td>${emp.name}</td>
                <td>${emp.categoryNumber}</td>
                <td>${emp.areaName}</td>
                <td>${emp.secretariatName}</td>
                <td>
                    <button class="btn btn-warning btn-sm" onclick="editEmployee(${emp.employeeId})">Editar</button>
                </td>
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
        const columnMap = {
            0: 'recordNumber',
            1: 'lastName',
            2: 'name',
            3: 'categoryNumber',
            4: 'areaName',
            5: 'secretariatName'
        };

        const sortKey = columnMap[columnIndex];
        if (!sortKey) return; // Salir si el índice no es válido

        const ths = document.querySelectorAll('#employeesTable th');
        const direction = ths[columnIndex].dataset.direction === 'asc' ? 'desc' : 'asc';

        ths.forEach(th => th.removeAttribute('data-direction'));
        ths[columnIndex].setAttribute('data-direction', direction);

        employeesData.sort((a, b) => {
            const aValue = a[sortKey] || ''; // Manejar valores nulos o indefinidos
            const bValue = b[sortKey] || '';
            if (typeof aValue === 'string' && typeof bValue === 'string') {
                return direction === 'asc'
                    ? aValue.localeCompare(bValue)
                    : bValue.localeCompare(aValue);
            }
            return direction === 'asc' ? aValue - bValue : bValue - aValue;
        });
        renderTable(employeesData);
    };

});

// Formulario de Carga de Empleados
const formEmployee = document.getElementById('formEmployee');
const btnAddEmployee = document.getElementById('btnAddEmployee');

if (formEmployee) {
    formEmployee.addEventListener('submit', function (e) {
        e.preventDefault();
        const formData = new FormData(formEmployee);
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

    document.getElementById("recordNumber").addEventListener("blur", function () {
        const legajoInput = this.value;

        // Validar que sea un número de 3 dígitos
        if (!/^\d{3}$/.test(legajoInput)) {
            Swal.fire({
                title: "Error",
                text: "El Legajo debe ser un número de 3 dígitos.",
                icon: "error",
                confirmButtonText: "OK"
            });
            this.value = "";
            return;
        }

        // CORRECCIÓN: Cambiar CheckLegajo por CheckRecordNumber
        fetch(`/Employee/CheckRecordNumber?recordNumber=${legajoInput}`)
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
                        <p><strong>${data.employee.Name} ${data.employee.LastName}</strong></p>
                        <p>Área: ${data.employee.AreaName}</p>
                        <p>Secretaría: ${data.employee.SecretariatName}</p>
                        <p>Para agregar a este empleado tu área, debes solicitar la transferencia a su superior.</p>
                    `,
                        icon: "warning",
                        confirmButtonText: "OK"
                    });
                    document.getElementById("recordNumber").value = ""; // Limpiar el campo
                }
            })
            .catch(error => console.error("Error al verificar el legajo:", error));
    });
}

function loadForm() {
    formEmployee.classList.toggle("hidden");
    btnAddEmployee.classList.toggle("hidden");

    if (!formEmployee.classList.contains("hidden")) {
        // Solicitar datos filtrados según el usuario logueado
        fetch("/Employee/GetAreasAndSecretariats")
            .then((response) => response.json())
            .then((data) => {
                loadOptionsWithSelection(data);
            })
            .catch((error) => console.error("Error al cargar las opciones:", error));
    }
}

function loadOptionsWithSelection(data) {
    // Cargar áreas
    const areaSelect = document.getElementById("areaId");
    areaSelect.innerHTML = '<option value="" disabled>Seleccione un área</option>';
    data.areas.forEach((area) => {
        const selected = data.defaultAreaId === area.id ? "selected" : "";
        areaSelect.innerHTML += `<option value="${area.id}" ${selected}>${area.name}</option>`;
    });

    // CORRECCIÓN: Cambiar secretarias por secretariats
    const secretariatSelect = document.getElementById("secretariatId");
    secretariatSelect.innerHTML = '<option value="" disabled>Seleccione una secretaría</option>';
    data.secretariats.forEach((secretariat) => {
        const selected = data.defaultSecretariatId === secretariat.id ? "selected" : "";
        secretariatSelect.innerHTML += `<option value="${secretariat.id}" ${selected}>${secretariat.name}</option>`;
    });

    // Cargar categorias
    const categorySelect = document.getElementById("categoryId");
    categorySelect.innerHTML = '<option value="" disabled>Seleccione una categoria</option>';
    data.categories.forEach((category) => {
        const selected = data.defaultCategoriaId === category.id ? "selected" : "";
        categorySelect.innerHTML += `<option value="${category.id}" ${selected}>${category.name}</option>`;
    });
}

function toggleForm() {
    formEmployee.classList.toggle('hidden');
    btnAddEmployee.classList.toggle('hidden');
}

// CORRECCIÓN: Cambiar cargarFormulario por loadForm en la vista
window.cargarFormulario = loadForm;

// Editar datos de un employee
function editEmployee(employeeId) {
    // Obtener los datos del employee desde el servidor
    fetch(`/Employee/GetEmployeeById?id=${employeeId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            // Generar formulario pre-cargado
            const formHtml = `
                <form id="formEmployeeEdit">
                    <div class="mb-3">
                        <label for="name" class="form-label">Nombre</label>
                        <input type="text" class="form-control" id="name" name="Name" value="${data.name}" required>
                    </div>
                    <div class="mb-3">
                        <label for="lastName" class="form-label">Apellido</label>
                        <input type="text" class="form-control" id="lastName" name="LastName" value="${data.lastname}" required>
                    </div>
                    <div class="mb-3">
                        <label for="categoryId" class="form-label">Categoría Salarial</label>
                        <select class="form-select" id="categoryId" name="SalaryCategoryId" required>
                            ${data.categories.map(c => `
                                <option value="${c.SalaryCategoryId}" ${c.SalaryCategoryId === data.categoryId ? "selected" : ""}>
                                    ${c.Number}
                                </option>`).join("")}
                        </select>
                    </div>
                    <div class="mb-3">
                        <label for="areaId" class="form-label">Área</label>
                        <select class="form-select" id="areaId" name="AreaId" required>
                            ${data.areas.map(a => `
                                <option value="${a.AreaId}" ${a.AreaId === data.areaId ? "selected" : ""}>
                                    ${a.Name}
                                </option>`).join("")}
                        </select>
                    </div>
                    <div class="mb-3">
                        <label for="secretariatId" class="form-label">Secretaría</label>
                        <select class="form-select" id="secretariatId" name="SecretariatId" required>
                            ${data.secretariats.map(s => `
                                <option value="${s.SecretariatId}" ${s.SecretariatId === data.secretariatId ? "selected" : ""}>
                                    ${s.Name}
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
                    const form = document.getElementById("formEmployeeEdit");
                    const formData = new FormData(form);

                    // Enviar los datos actualizados al servidor
                    return fetch(`/Employee/EditEmployee?id=${employeeId}`, {
                        method: "POST",
                        body: formData,
                        headers: {
                            "X-Requested-With": "XMLHttpRequest",
                            "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value
                        }
                    })
                        .then(response => {
                            if (!response.ok) {
                                throw new Error("Error al actualizar el employee.");
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
                text: `No se pudo cargar el employee: ${error.message}`,
                icon: "error",
                confirmButtonText: "OK"
            });
        });
}