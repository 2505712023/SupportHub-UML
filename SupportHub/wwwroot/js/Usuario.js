
//autocompletado de los empleados
const inputt = document.getElementById('nombre');
const inputBuscador = document.getElementById('inputBuscador');
const suggestionsDiv1 = document.getElementById('suggestions1');
const suggestionsDiv2 = document.getElementById('suggestions2');


inputt.addEventListener('input', BuscarCoincidencias);
inputBuscador.addEventListener('input', BuscarCoincidencias);
suggestionsDiv2.addEventListener('click', function (event) {
    if (event.target.classList.contains('suggestion-item')) {
        inputt.value = event.target.textContent;
        suggestionsDiv2.innerHTML = '';
    }
});
suggestionsDiv1.addEventListener('click', function (event) {
    if (event.target.classList.contains('suggestion-item')) {
        inputBuscador.value = event.target.textContent;
        suggestionsDiv1.innerHTML = '';
    }
});

function BuscarCoincidencias() {
    const searchTerm = inputt.value;
    const searchTerm2 = inputBuscador.value;
    let argumentoDeBusqueda = "";
    if (searchTerm.length > 0 || searchTerm2.length > 0) {

        if (searchTerm.length > 0) {
            argumentoDeBusqueda = searchTerm;
        }
        else if (searchTerm2.length > 0) {
            argumentoDeBusqueda = searchTerm2;
        }

        fetch(`/Usuario/mostrarUsuarios?handler=Empleados&term=${encodeURIComponent(argumentoDeBusqueda)}`)
            .then(response => response.json())
            .then(data => {
                if (searchTerm.length > 0) {
                    console.log('datos recibidos: ', data);
                    suggestionsDiv2.innerHTML = '';
                    data.forEach(item => {
                        console.log('item :', item);
                        const div = document.createElement('div');
                        div.textContent = item.nombreCompleto;
                        console.log('agregando:', div.textContent);
                        div.classList.add('suggestion-item');

                        suggestionsDiv2.appendChild(div);
                    });
                }
                else if (searchTerm2.length > 0) {
                    console.log('datos recibidos: ', data);
                    suggestionsDiv1.innerHTML = '';
                    data.forEach(item => {
                        console.log('item :', item);
                        const div = document.createElement('div');
                        div.textContent = item.nombreCompleto;
                        console.log('agregando:', div.textContent);
                        div.classList.add('suggestion-item');

                        suggestionsDiv1.appendChild(div);
                    });
                }
            }).catch(error => console.error('Error de fetch en: ', error));
    } else {
        suggestionsDiv1.innerHTML = '';
        suggestionsDiv2.innerHTML = '';
    }
}
//paginación de la tabla y manejo del type de los inputs password y los íconos de ojito
document.addEventListener("DOMContentLoaded", function () {
    const rowsPerPage = 8; // Mostrar 8 usuarios por página
    const tableBody = document.getElementById("TBody");
    const rows = Array.from(tableBody.querySelectorAll("tr"));
    const paginationList = document.querySelector(".pagination-list");
    const totalPages = Math.ceil(rows.length / rowsPerPage);

    function showPage(pageNumber) {
        // Limpiar las filas actuales
        tableBody.innerHTML = "";

        // Calcular los índices de las filas para la página actual
        const startIndex = (pageNumber - 1) * rowsPerPage;
        const endIndex = Math.min(startIndex + rowsPerPage, rows.length);
        const pageRows = rows.slice(startIndex, endIndex);

        // Agregar las filas de la página actual
        pageRows.forEach(row => {
            tableBody.appendChild(row);
        });

        // Actualizar los puntos de paginación
        updatePagination(pageNumber);
    }

    function updatePagination(currentPage) {
        paginationList.innerHTML = ""; // Limpiar la paginación actual

        for (let i = 1; i <= totalPages; i++) {
            const pageItem = document.createElement("li");
            pageItem.className = "page-item";

            const pageDot = document.createElement("span");
            pageDot.className = "page-dot";

            if (i === currentPage) {
                pageDot.classList.add("active");
            }

            pageDot.addEventListener("click", function () {
                showPage(i);
            });

            pageItem.appendChild(pageDot);
            paginationList.appendChild(pageItem);
        }
    }

    // Mostrar la primera página al cargar
    showPage(1);

    // mostrar y ocultar la contraseña
    const input1 = document.getElementById("contraseña"),
        input2 = document.getElementById("contra2"),
        ojo1 = document.getElementById("ojo1"),
        ojo2 = document.getElementById("ojo2");

    if (ojo1 && ojo2 && input1 && input2) {
        ojo1.addEventListener('click', () => {
            if (input1.type === "password") {
                input1.type = "text";
                ojo1.classList.remove("bi-eye-slash");
                ojo1.classList.add("bi-eye");
            }
            else {
                input1.type = "password";
                ojo1.classList.remove("bi-eye");
                ojo1.classList.add("bi-eye-slash");
            }
        });

        ojo2.addEventListener('click', () => {
            if (input2.type === "password") {
                input2.type = "text";
                ojo2.classList.remove("bi-eye-slash");
                ojo2.classList.add("bi-eye");
            }
            else {
                input2.type = "password";
                ojo2.classList.remove("bi-eye");
                ojo2.classList.add("bi-eye-slash");
            }
        });
    }
});

//Scripts para el modal y el formulario

function llenarModalModificarUsuario(button) {
    var tr = $(button).closest("tr");
    $(".modal #esModificacion").val("true");
    $(".modal h1").text("Modificar Usuario");
    $(".modal #id").val(tr.data("id"));
    $(".modal #usuario").val(tr.data("usuario"));
    $(".modal #activo").val(tr.data("activo"));
    $(".modal #nombre").val(tr.data("empleado"));
    $(".modal #contraseña").val(tr.data("contra"));
    $(".modal #TipoUsuario").val(tr.data("rol"));

    $(".modal #contraseña").prop("type", "password");
    $(".modal #activo").prop("checked", tr.data("activo") == 'True');

}

function limpiarModalModificarUsuario() {
    //const suggestionsDiv = document.getElementById('suggestions');
    suggestionsDiv2.innerHTML = '';
    $(".modal #esModificacion").val("false")
    $(".modal h1").text("Agregar Usuario");
    $("#formAgregarUsuario")[0].reset();

    $(".modal #nombre").prop("readonly", false);
    $('label[for="TipoUsuario"]').show();
    $('label[for="contraseña"]').show();
    //poniendo los inputs y ojos en su estado inicial
    $('#contraseña').prop('type','password');
    $('#contra2').prop('type', 'password');
    $('#ojo1').removeClass("bi-eye").addClass("bi-eye-slash");
    $('#ojo2').removeClass("bi-eye").addClass("bi-eye-slash");
}

function limpiarModalEliminar() {
    $(".modal#Eliminar #esEliminacion").val("false");
    $("#formEliminarUsuario")[0].reset();
}

function llenarModalEliminar(button) {
    var tr = $(button).closest("tr");

    $(".modal#Eliminar #usuario").val(tr.data("usuario"));
    $(".modal h1").text("Eliminar Usuario");
    $(".modal#Eliminar #esEliminacion").val("true");
    $("#Eliminar").on("shown.bs.modal", function () {
        $("#btnEliminar").focus();
    });
}

function validarFormularioUsuario() {
    var usuario = document.getElementById("usuario").value.trim();
    var codEmpleado = document.getElementById("nombre").value.trim();
    var activo = document.getElementById("activo").value.trim();
    var contra = document.getElementById("contraseña").value.trim();
    var contra2 = document.getElementById("contra2").value.trim();
    var rol = document.getElementById("TipoUsuario").value.trim();
    let longitudCodigo = 7;
    if (usuario === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Nombre de usuario es requerido!"
        });
        return false;
    } else if (usuario.length > 25) {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "El nombre de usuario solo permite 25 caracteres!"
        });
        return false;
    } else if (codEmpleado == "" || codEmpleado.length < longitudCodigo) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Debe especificar al menos el código de empleado con 7 caracteres"
        });
        return false;
    } else if (rol == null || rol == "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Debe especificar el rol del usuario"
        });
        return false;
    } else if ((contra == null || contra == "" )&& (contra2 != null || contra2 != "")) {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Asegúrese de llenar ambos campos de contraseña"
        });
        return false;
    } else if ((contra !== "" && contra2 !== "") && (contra !== contra2)) {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Las contraseñas no coinciden"
        });
        return false;
    } else if (contra != "" && !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/.test(contra)) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "La nueva contraseña debe tener al menos 8 caracteres, incluyendo mayúsculas, minúsculas, números y caracteres especiales."
        });
        return false;
    } 

    document.getElementById("formAgregarUsuario").submit();
}


function validarFormularioEliminar() {
    if ($(".modal#Eliminar #usuario").val().trim() === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Usuario es requerido!"
        });
        return false;
    }
    submitFormEliminar()
}

function submitFormEliminar() {
    document.getElementById("formEliminarUsuario").submit();
}