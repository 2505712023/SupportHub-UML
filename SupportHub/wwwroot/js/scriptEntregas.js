// Inicializar tooltips según bootstrap
const tooltipTriggerList = document.querySelectorAll('[data-bs-placement]');
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

$(document).ready(function () {
    const rowsPerPage = 10; // Mostrar 10 empleados por página
    const tableBody = document.getElementById("entregasBody");
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
        ejecutarBusqueda();
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

    console.log("Document ready!");
    $("#busqueda").focus().select();

    if (localStorage.getItem("filtro-entregas") != null) {
        $("#filtros").val(localStorage.getItem("filtro-entregas"));
    }

    $("#busqueda").on("keyup", function () {
        ejecutarBusqueda();
    });

    $("#busqueda").on("input", function () {
        if ($("#busqueda").val() === '') {
            $("#btn-limpiar-filtro").hide();
        } else {
            $("#btn-limpiar-filtro").show();
        }
    });

    $('tr[data-id-entrega]').each(function () {
        var row = $(this);
        var fechaDevolucion = row.find('#fechaDevolucion');
        var modificar = row.find(".btn-edit");

        if (fechaDevolucion.text().trim() !== "") {
            modificar.attr("disabled", true);
            modificar.addClass("text-decoration-line-through");
        }
    });
    
    $("#filtros").on("change", function () {
        localStorage.setItem("filtro-entregas", $("#filtros").val());
        $("#busqueda").focus().select();
        ejecutarBusqueda();
    });

    $("#formIdEquipo").on("change", function () {
        $("#formCantidadEntrega").val("0");
    });
});

function ejecutarBusqueda() {
    var filtroSeleccionado = $("#filtros").val();
    var valorAFiltrar = $("#busqueda").val().toLowerCase();
    $("#tablaEntregas tbody tr").filter(function () {
        var textoColumna = $(this).find('td').eq(filtroSeleccionado).text().toLowerCase();

        $(this).toggle(textoColumna.indexOf(valorAFiltrar) > -1);
    });
}

$(document).on('input', '#formCantidadEntrega', function () {
    this.value = Math.min(this.value, 1000000)
    actualizarAlertaCantidadDisponible();
});

$(document).on('change', '#formIdEquipo', function () {
    actualizarAlertaCantidadDisponible();
});

function actualizarAlertaCantidadDisponible() {
    var cantidad = parseInt($("#formCantidadEntrega").val());

    if (!isNaN(cantidad) && parseInt($("#formIdEquipo option:selected").data("disponible")) < cantidad) {
        $("#alertaCantidadDisponible").show();
        $("#modalActionButton").attr("disabled", true);
    } else {
        $("#alertaCantidadDisponible").hide();
        $("#modalActionButton").attr("disabled", false);
    }
}

function openModal(opcion, button = null) {

    // Guardamos los selects del modal
    var selectIdTipoEntrega = $("#formIdTipoEntrega").closest("select").prop("outerHTML");
    var selectIdEmpleadoEntrega = $("#formIdEmpleadoEntrega").closest("select").prop("outerHTML");
    var selectIdEmpleadoRecibe = $("#formIdEmpleadoRecibe").closest("select").prop("outerHTML");
    var selectIdEquipo = $("#formIdEquipo").closest("select").prop("outerHTML");

    //Eliminamos los selects del modal ya que los vamos a insertar nuevamente pero en diferente orden
    $("#formIdTipoEntrega").closest("select").remove();
    $("#formIdEmpleadoEntrega").closest("select").remove();
    $("#formIdEmpleadoRecibe").closest("select").remove();
    $("#formIdEquipo").closest("select").remove();

    if (button === null) {
        if (opcion === "agregar") {
            $("#modalActionButton").text("Agregar");
            $(".modal h1").text("Agregar Entrega");
            $("#modalBody").html(`
                <label class="col-sm-12 col-form-label">Tipo de entrega:</label>
                <div class="col-sm-12">
                    ${selectIdTipoEntrega}
                </div>

                <label class="col-sm-12 col-form-label">Equipo:</label>
                <div class="col-sm-12">
                    ${selectIdEquipo}
                </div>

                <label class="col-sm-12 col-form-label">Cantidad:</label>
                <div class="col-sm-12">
                    <input type="number" class="form-control" name="cantidadEntrega" placeholder="0" step="1" min="1" max="1000000"  id="formCantidadEntrega" />
                </div>

                <div class="alert alert-danger mt-3 mb-0" role="alert" id="alertaCantidadDisponible" style="display: none;">
                        <i class="bi bi-exclamation-diamond-fill"></i> No hay suficiente cantidad disponible de ese equipo!
                </div>

                <label class="col-sm-12 col-form-label">Fecha:</label>
                <div class="col-sm-12">
                    <input type="date" class="form-control" name="fechaEntrega" id="formFechaEntrega" />
                </div>

                <label class="col-sm-12 col-form-label">Empleado entrega:</label>
                <div class="col-sm-12">
                    ${selectIdEmpleadoEntrega}
                </div>

                <label class="col-sm-12 col-form-label">Empleado recibe:</label>
                <div class="col-sm-12">
                    ${selectIdEmpleadoRecibe}
                </div>

                <label class="col-sm-12 col-form-label">Observaciones:</label>
                <div class="col-sm-12">
                    <textarea class="form-control" name="observacionEntrega" placeholder="Agregar observación (opcional)..." id="formObservacionEntrega" rows="3" ></textarea>
                </div>
            `);
            $("#formFechaEntrega").val(new Date().toLocaleDateString('en-CA'));
            mostrarSelects();
        }
    } else {
        if (opcion === "modificar") {

            $("#modalActionButton").text("Guardar");
            $("#modalBody").html(`
                <input type="hidden" id="idEntrega" name="idEntrega" value="" />

                <input type="hidden" id="esModificacion" name="esModificacion" value="true" />

                <label class="col-sm-12 col-form-label">Código:</label>
                <div class="col-sm-12">
                    <input type="text" class="form-control" id="codEntrega" name="codEntrega" >
                </div>

                <label class="col-sm-12 col-form-label">Tipo de entrega:</label>
                <div class="col-sm-12">
                    ${selectIdTipoEntrega}
                </div>

                <label class="col-sm-12 col-form-label">Equipo:</label>
                <div class="col-sm-12">
                    ${selectIdEquipo}
                </div>

                <label class="col-sm-12 col-form-label">Cantidad:</label>
                <div class="col-sm-12">
                    <input type="number" class="form-control" name="cantidadEntrega" placeholder="0" id="formCantidadEntrega" />
                </div>

                <div class="alert alert-danger mt-3 mb-0" role="alert" id="alertaCantidadDisponible" style="display: none;">
                        <i class="bi bi-exclamation-diamond-fill"></i> No hay suficiente cantidad disponible de ese equipo!
                </div>

                <label class="col-sm-12 col-form-label">Fecha:</label>
                <div class="col-sm-12">
                    <input type="date" class="form-control" name="fechaEntrega" id="formFechaEntrega" />
                </div>

                <label class="col-sm-12 col-form-label">Empleado entrega:</label>
                <div class="col-sm-12">
                    <input type="text" class="form-control" name="nombreEmpleadoEntrega" id="formNombreEmpleadoEntrega" disabled />
                </div>

                <label class="col-sm-12 col-form-label">Empleado recibe:</label>
                <div class="col-sm-12">
                    ${selectIdEmpleadoRecibe}
                </div>

                <label class="col-sm-12 col-form-label">Observaciones:</label>
                <div class="col-sm-12">
                    <textarea class="form-control" name="observacionEntrega" placeholder="Agregar observación (opcional)..." id="formObservacionEntrega" rows="3" ></textarea>
                </div>
                ${selectIdEmpleadoEntrega}
            `);
            mostrarSelects();
            $(".modal #formIdEmpleadoEntrega").hide();

            var tr = $(button).closest("tr");
            $(".modal h1").text("Modificar Entrega");
            $(".modal #idEntrega").val(tr.data("idEntrega"));
            $(".modal #codEntrega").val(tr.data("codEntrega"));
            $(".modal #codEntrega").prop("readonly", true);
            $(".modal #formIdTipoEntrega").val(tr.data("idTipoEntrega"));
            $(".modal #formIdEquipo").val(tr.data("idEquipo"));
            $(".modal #formCantidadEntrega").val(tr.data("cantidadEntrega"));
            $(".modal #formFechaEntrega").val(tr.data("fechaEntrega"));
            $(".modal #formNombreEmpleadoEntrega").val(tr.data("nombreEmpleadoEntrega"));
            $(".modal #formIdEmpleadoRecibe").val(tr.data("idEmpleadoRecibe"));
            $(".modal #formObservacionEntrega").val(tr.data("observacionEntrega"));

            // Obteniendo la cantidad anterior y actualizando el nuevo disponible
            var equipoSeleccionado = $(".modal #formIdEquipo option:selected");
            var cantidadEntregaAnterior = parseInt($(".modal #formCantidadEntrega").val(), 10) || 0;
            var cantidadDisponibleAnterior = parseInt(equipoSeleccionado.data("disponible"), 10) || 0;
            var nuevaCantidadDisponible = cantidadEntregaAnterior + cantidadDisponibleAnterior;

            // Obteniendo el texto anterior y actualizando el nuevo texto
            var textoAnterior = $(".modal #formIdEquipo option:selected").text();
            var nuevoTexto = textoAnterior.replace(/\(\d+ disponibles\)/, '(' + nuevaCantidadDisponible + ' disponibles)');

            if (tr.data("fechaDevolucion") === "") {
                // Actualizando valores del select luego de haber calculado la nueva cantidad y haber definido el nuevo texto
                $(".modal #formIdEquipo option:selected").attr("data-disponible", nuevaCantidadDisponible).data("disponible", nuevaCantidadDisponible);
                $(".modal #formIdEquipo option:selected").text(nuevoTexto);
            }

            // Guardando cambios en localStorage por si se necesitan revertir posteriormente sin guardar una modificación de entrega
            localStorage.setItem('cantidadDisponibleAnterior', cantidadDisponibleAnterior);
            localStorage.setItem('textoAnterior', textoAnterior);

        } else if (opcion === "eliminar") {
            $("#modalActionButton").text("Eliminar");
            $("#modalActionButton").removeClass("btn-primary");
            $("#modalActionButton").addClass("btn-danger");
            $("#modalBody").html(`
                <p>¿Seguro que desea eliminar una entrega?</p>
                <input type="hidden" name="codEntrega" id="codEntrega" value="" />
                <input type="hidden" name="esEliminacion" id="esEliminacion" value="true" />
                ${selectIdTipoEntrega}
                ${selectIdEquipo}
                ${selectIdEmpleadoEntrega}
                ${selectIdEmpleadoRecibe}
            `);

            var tr = $(button).closest("tr");
            $(".modal h1").text("Eliminar Entrega");
            $(".modal #codEntrega").val(tr.data("codEntrega"));
            ocultarSelects();

        } else if (opcion === "agregarDevolucion") {
            $("#modalActionButton").text("Guardar");
            $("#modalBody").html(`
                <input type="hidden" name="codEntrega" id="codEntrega" value="" />
                <input type="hidden" name="esDevolucion" id="esDevolucion" value="true" />
                <input type="hidden" name="fechaEntrega" id="fechaEntrega" value="" />

                <label class="col-sm-12 col-form-label">Fecha devolución:</label>
                <div class="col-sm-12">
                    <input type="date" class="form-control" name="fechaDevolucion" id="formFechaDevolucion" />
                </div>

                ${selectIdTipoEntrega}
                ${selectIdEquipo}
                ${selectIdEmpleadoEntrega}
                ${selectIdEmpleadoRecibe}
            `);

            var tr = $(button).closest("tr");
            $(".modal h1").text("Asignar Devolución");
            $(".modal #codEntrega").val(tr.data("codEntrega"));
            $(".modal #fechaEntrega").val(tr.data("fechaEntrega"));
            $("#formFechaDevolucion").val(new Date().toLocaleDateString('en-CA'));
            ocultarSelects();

        } else if (opcion === "eliminarDevolucion") {
            $("#modalActionButton").text("Eliminar");
            $("#modalActionButton").removeClass("btn-primary");
            $("#modalActionButton").addClass("btn-danger");
            $("#modalBody").html(`
                <p>¿Seguro que desea eliminar la fecha de devolución de la entrega?</p>
                <input type="hidden" name="codEntrega" id="codEntrega" value="" />
                <input type="hidden" name="esEliminacionDevolucion" id="esEliminacionDevolucion" value="true" />
                ${selectIdTipoEntrega}
                ${selectIdEquipo}
                ${selectIdEmpleadoEntrega}
                ${selectIdEmpleadoRecibe}
            `);

            var tr = $(button).closest("tr");
            $(".modal h1").text("Eliminar Devolución");
            $(".modal #codEntrega").val(tr.data("codEntrega"));
            ocultarSelects();
        }
    }
}

function submitFormEntregas() {
    if ($(".modal #esEliminacion").val() === "true") {
        $("#formEntregas").submit();

    } else if ($(".modal #esDevolucion").val() === "true") {
        if ($("#formFechaDevolucion").val().trim() === "") {
            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Fecha devolución es requerido!"
            });
            return false;
        } else if (new Date($("#formFechaDevolucion").val()) < new Date($("#fechaEntrega").val())) {
            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Fecha devolución no puede ser menor a la fecha de entrega!"
            });
            return false;
        } else if (new Date($("#formFechaDevolucion").val()) > new Date()) {
            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Fecha devolución no puede ser posterior a este día!"
            });
            return false;
        }

        $("#formEntregas").submit();

    } else if ($(".modal #esEliminacionDevolucion").val() === "true") {
        $("#formEntregas").submit();

    } else {
        if ($(".modal #esModificacion").val() !== "true") {
            $("#formIdEmpleadoEntrega").prop("disabled", false)
        }

        if ($("#formIdTipoEntrega").val().trim() === "") {

            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Tipo de entrega es requerido!"
            });
            return false;

        } else if ($("#formIdEquipo").val().trim() === "") {

            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Equipo es requerido!"
            });
            return false;

        } else if ($("#formCantidadEntrega").val() <= 0) {

            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Cantidad debe ser mayor a 0!"
            });
            return false;

        } else if ($("#formCantidadEntrega").val() != parseInt($("#formCantidadEntrega").val())) {

            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Cantidad debe ser un número entero!"
            });
            return false;

        } else if ($("#formFechaEntrega").val().trim() === "") {

            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Fecha entrega es requerido!"
            });
            return false;

        } else if (new Date($("#formFechaEntrega").val()) < (new Date(`${new Date().getFullYear()}-01-01`))) {

            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Fecha entrega debe ser en el año actual!"
            });
            return false;

        } else if (new Date($("#formFechaEntrega").val()) > new Date()) {

            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Fecha entrega no puede ser posterior a este día!"
            });
            return false;

        } else if ($("#formIdEmpleadoEntrega").val().trim() === "") {

            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Empleado entrega es requerido!"
            });
            return false;

        } else if ($("#formIdEmpleadoRecibe").val().trim() === "") {

            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Empleado recibe es requerido!"
            });
            return false;

        }

        $("#formEntregas").submit();
    }
}

function limpiarFiltroEntregas() {
    $("#busqueda").val("");
    $("#tablaEntregas tbody tr").show();
    $("#btn-limpiar-filtro").hide();
    $("#busqueda").focus().select();
}

function limpiarModalEntregas() {
    if ($(".modal #esEliminacion").val() === "true" || $(".modal #esEliminacionDevolucion").val() === "true") {
        $("#modalActionButton").removeClass("btn-danger");
        $("#modalActionButton").addClass("btn-primary");
    }

    if ($(".modal #esModificacion").val() === "true") {
        // Buscamos los valores que se guardaron en localStorage
        cantidadDisponibleAnterior = localStorage.getItem("cantidadDisponibleAnterior");
        textoAnterior = localStorage.getItem("textoAnterior");

        // Guardamos el option:selected
        var opcionSeleccionada = $(".modal #formIdEquipo option:selected");

        // Creamos una nueva opción modificada en base a lo anterior
        let opcionAnterior = $('<option>')
            .attr("value", opcionSeleccionada.val())
            .attr("data-disponible", cantidadDisponibleAnterior)
            .text(textoAnterior);

        // Intercambiamos la opción seleccionada por la nueva opción
        opcionSeleccionada.replaceWith(opcionAnterior);

        // Detonamos el refresco de la UI
        $(".modal #formIdEquipo").val(opcionAnterior.val()).trigger("change");
    }

    $("#modalEntregas h1").text("Agregar Entrega");
    $("#formEntregas")[0].reset();
}

function mostrarSelects() {
    $(".modal #formIdTipoEntrega").show();
    $(".modal #formIdEquipo").show();
    $(".modal #formIdEmpleadoEntrega").show();
    $(".modal #formIdEmpleadoRecibe").show();
}

function ocultarSelects() {
    $(".modal #formIdTipoEntrega").hide();
    $(".modal #formIdEquipo").hide();
    $(".modal #formIdEmpleadoEntrega").hide();
    $(".modal #formIdEmpleadoRecibe").hide();
}
