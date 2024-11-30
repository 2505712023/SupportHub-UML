document.addEventListener("DOMContentLoaded", function () {
    const rowsPerPage = 10; // Mostrar 10 empleados por página
    const tableBody = document.getElementById("equiposBody");
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
    showPage(1);
});

function validarFormularioEmpleado() {
    var tipoequipo = document.getElementById("tipoequipo").value.trim();
    var marcaequipo = document.getElementById("marcaequipo").value.trim();
    var modeloequipo = document.getElementById("modeloequipo").value.trim();
    var cantidadequipo = document.getElementById("cantidadequipo").value.trim();
    var precioequipo = document.getElementById('precioequipo').value;
    var idProveedor = document.getElementById('idProveedor').value;
    var descripcionequipo = document.getElementById('descripcionequipo').value;
 
    if (tipoequipo === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Tipo de equipo es requerido!"
        });
        return false;
    } else if (marcaequipo === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Marca de equipo es requerido!"
        });
        return false;
    } else if (cantidadequipo === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Cantidad es requerido!"
        });
        return false;
    } else if (cantidadequipo != parseInt(cantidadequipo)) {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Cantidad debe ser un número entero!"
        });
        return false;
    } else if (precioequipo === "") {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "El precio es requerido."
        });
        return false;
    } else if (!idProveedor ) {
        Swal.fire({
            title: "Error",
            text: "Debe seleccionar un proveedor.",
            icon: "error",
            confirmButtonText: "OK"
        });
        return false;
    } else if (descripcionequipo === "") {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text:"La descrición es requerido."
        });
        return false;
    }

    // Enviar el formulario si todo está correcto
    document.getElementById('formAgregarEquipo').submit();
}

function limpiarModalEquipo() {
    $(".modal #esModificacion").val("false")
    $(".modal h1").text("Agregar Equipo");
    $("#formAgregarEquipo")[0].reset();
}

function llenarModal(button) {
    var tr = $(button).closest("tr");
    var data = tr.data();
    console.log(data);
    $(".modal #esModificacion").val("true");
    $(".modal h1").text("Modificar Equipo");
    $(".modal #id").val(tr.data("id"));
    $(".modal #codigo").val(tr.data("codigo"));
    $(".modal #tipoequipo").val(tr.data("tipoequipo"));
    $(".modal #marcaequipo").val(tr.data("marcaequipo"));
    $(".modal #modeloequipo").val(tr.data("modeloequipo"));
    $(".modal #cantidadequipo").val(tr.data("cantidadequipo"));
    $(".modal #precioequipo").val(tr.data("precioequipo"));

    // Selecciona la opción correcta en el combobox de Proveedor
    $(".modal #idProveedor").val(tr.data("idproveedor")).change();

    $(".modal #descripcionequipo").val(tr.data("descripcionequipo"));

    $(".modal #codigo").prop("readonly", true);
}

function llenarModalEliminar(button) {
    var tr = $(button).closest("tr");

    $(".modal h1").text("Eliminar Equipo");
    $(".modal#Eliminar #codigo").val(tr.data("codigo"));
    $(".modal#Eliminar #esEliminacion").val("true");
    $("#Eliminar").on("shown.bs.modal", function () {
        $("#btnEliminar").focus();
    });
}

function limpiarModalEliminar() {
    $(".modal h1").text("Agregar Equipo");
    $(".modal#Eliminar #esEliminacion").val("false");
    $("#formEliminarEquipo")[0].reset();
}

function validarFormularioEliminar() {
    if ($(".modal#Eliminar #codigo").val().trim() === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "codigo es requerido!"
        });
        return false;
    }
    submitFormEliminar()
}

function submitFormEliminar() {
    document.getElementById("formEliminarEquipo").submit();
}