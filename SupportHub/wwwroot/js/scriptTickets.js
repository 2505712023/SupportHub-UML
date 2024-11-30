let fechaCreacion = null;

// Inicializar tooltips según bootstrap
const tooltipTriggerList = document.querySelectorAll('[data-bs-placement]');
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

//Variables iniciales para el manejo de la tabla principal de la vista
var rowsOriginales = [];
var filasPorPagina = 10;
var paginasTotales = 0;

function showImageInModal(imageUrl) {
    // Cambiar el atributo 'src' de la imagen dentro del modal
    document.getElementById('modalImage').src = imageUrl;
    // Mostrar el modal
    $('#imageModal').modal('show');
}

$(document).ready(function () {
    rowsOriginales = Array.from(document.getElementById("ticketsBody").querySelectorAll("tr")); // Convert NodeList to Array
    mostrarPagina(1, filasPorPagina); // Inicializa la paginación con 10 filas por página
    filtrarFilasOriginales();
});

function mostrarPagina(numeroPagina, filasPorPagina) {
    // Filter to include only visible rows
    const rowsVisibles = rowsOriginales.filter(row => row.style.display !== "none");

    paginasTotales = Math.ceil(rowsVisibles.length / filasPorPagina); // Calcular el total de páginas
    const tableBody = document.getElementById("ticketsBody");

    // Limpiar las filas actuales
    tableBody.innerHTML = "";

    // Calcular los índices de las filas para la página actual
    const indiceInicial = (numeroPagina - 1) * filasPorPagina;
    const indiceFinal = Math.min(indiceInicial + filasPorPagina, rowsVisibles.length);
    const filasDePaginaActual = rowsVisibles.slice(indiceInicial, indiceFinal); // Filas a mostrar

    // Agregar las filas de la página actual
    filasDePaginaActual.forEach(row => {
        tableBody.appendChild(row);
    });

    // Actualizar los puntos de paginación
    actualizarPaginacion(numeroPagina);
}

function actualizarPaginacion(paginaActual) {
    const listaPaginacion = document.querySelector(".pagination-list");
    listaPaginacion.innerHTML = ""; // Limpiar la paginación actual

    for (let i = 1; i <= paginasTotales; i++) {
        const pageItem = document.createElement("li");
        pageItem.className = "page-item";

        const pageDot = document.createElement("span");
        pageDot.className = "page-dot";

        if (i === paginaActual) {
            pageDot.classList.add("active");
        }

        pageDot.addEventListener("click", function () {
            mostrarPagina(i, filasPorPagina); // Puedes ajustar el número de filas por página aquí
        });

        pageItem.appendChild(pageDot);
        listaPaginacion.appendChild(pageItem);
    }
}

$(document).on("change", "#formIdItResponsable", function () {
    if ($(this).val() !== "") {
        $("#formIdEstado option").each(function () {
            if ($.trim($(this).text()) === "Asignado") {
                $(this).prop("selected", true);
            }
            $("#formIdEstado").prop("disabled", true);
        });
    } else {
        $("#formIdEstado option").each(function () {
            if ($.trim($(this).text()) === "Abierto") {
                $(this).prop("selected", true);
            }
        });
        $("#formIdEstado").prop("disabled", false);
    }
});

$(document).on("click", ".filtro-estados span", function () {
    const text = $(this).text();
    if (text === "Cualquiera") {
        if ($("#filtroEstadoTodos").hasClass("estado-todos-off")) {
            $("#filtroEstadoTodos").toggleClass("estado-todos estado-todos-off");

            //Quitar todas las clases de los demás span
            $("#filtroEstadoAbierto").removeClass("estado-abierto");
            $("#filtroEstadoAsignado").removeClass("estado-asignado");
            $("#filtroEstadoEnProceso").removeClass("estado-en-proceso");
            $("#filtroEstadoCerrado").removeClass("estado-cerrado");

            //Asignar las clases "off" a los demás span
            $("#filtroEstadoAbierto").addClass("estado-abierto-off");
            $("#filtroEstadoAsignado").addClass("estado-asignado-off");
            $("#filtroEstadoEnProceso").addClass("estado-en-proceso-off");
            $("#filtroEstadoCerrado").addClass("estado-cerrado-off");

        } else if ( /* Si todos los demás span están "off" no se puede hacer "off" a estado-todos */
            !$("#filtroEstadoAbierto").hasClass("estado-abierto-off")
            && !$("#filtroEstadoAsignado").hasClass("estado-asigando-off")
            && !$("#filtroEstadoEnProceso").hasClass("estado-en-proceso-off")
            && !$("#filtroEstadoCerrado").hasClass("estado-en-cerrado-off")
        ) {
            $(this).toggleClass("estado-todos estado-todos-off");
        }
    } else if (text === "Abierto") {
        if ($("#filtroEstadoTodos").hasClass("estado-todos")) {
            $("#filtroEstadoTodos").toggleClass("estado-todos estado-todos-off");
        }
        $(this).toggleClass("estado-abierto estado-abierto-off");
    } else if (text === "Asignado") {
        if ($("#filtroEstadoTodos").hasClass("estado-todos")) {
            $("#filtroEstadoTodos").toggleClass("estado-todos estado-todos-off");
        }
        $(this).toggleClass("estado-asignado estado-asignado-off");
    } else if (text === "En Proceso") {
        if ($("#filtroEstadoTodos").hasClass("estado-todos")) {
            $("#filtroEstadoTodos").toggleClass("estado-todos estado-todos-off");
        }
        $(this).toggleClass("estado-en-proceso estado-en-proceso-off");
    } else if (text === "Cerrado") {
        if ($("#filtroEstadoTodos").hasClass("estado-todos")) {
            $("#filtroEstadoTodos").toggleClass("estado-todos estado-todos-off");
        }
        $(this).toggleClass("estado-cerrado estado-cerrado-off");
    }
    filtrarFilasOriginales();
});

function filtrarFilasOriginales() {
    // Obtener los spans de filtro de estados
    const spansEstados = $(".filtro-estados span").not(function () {
        return $(this).attr("class").includes("-off");
    });

    // Verificar si el span "Todos" está activo
    const todosActivo = $(".filtro-estados .estado-todos").hasClass("estado-todos");

    // Filtrar la tabla en base a los estados activos
    rowsOriginales.forEach(row => {
        const $row = $(row); // Convertir cada fila en un elemento de jQuery
        const estadoFila = $row.data("estado");

        // Mostrar la fila si el filtro "Todos" está activo o si el estado de la fila coincide con alguno de los estados activos
        const mostrarFila = todosActivo || spansEstados.filter(function () {
            return $(this).text() === estadoFila;
        }).length > 0;

        $row.toggle(mostrarFila);
        mostrarPagina(1, filasPorPagina); // Inicializa la paginación con 10 filas por página
    });
}

function limpiarModalTickets() {
    $("#modalTickets h1").text("Crear Ticket");
    $("#formTickets")[0].reset();
    $("#previewImagenTicket").attr("src", "");
}

function openModal(opcion, button = null) {

    if (button === null) {
        if (opcion === "agregar") {
            //ocultando
            $(".modal #formfechaFinalizado").hide();
            $(".modal #reAbrir").hide();
            $(".modal #comenzar").hide();
            $(".modal label[for='formfechaFinalizado']").hide();

            //mostrando
            $(".modal #formTituloTicket").show();
            $(".modal #codTicket").show();
            $(".modal .agregar-modificar").show();
            $(".modal label[for='formTituloTicket']").show();
            $(".modal label[for='codTicket']").show();
            $("#modalActionButton").text("Crear");
            $(".modal h1").text("Crear Ticket");
            $(".modal label[for='codTicket']").hide();
            $(".modal #codTicket").hide();
            $(".modal #accion").val("agregar");
        }
    }
    else {
        if (opcion === "modificar") {
            //ocultando
            $(".modal #formfechaFinalizado").hide();
            $(".modal #reAbrir").hide();
            $(".modal #comenzar").hide();
            $(".modal label[for='formfechaFinalizado']").hide();

            //mostrando
            $(".modal #formTituloTicket").show();
            $(".modal #codTicket").show();
            $(".modal .agregar-modificar").show();

            $(".modal label[for='formTituloTicket']").show();
            $(".modal label[for='codTicket']").show();

            $("#modalActionButton").text("Modificar");
            $("#modalActionButton").css("background-color", "#37A668");
            $(".modal #accion").val("modificar");
            var tr = $(button).closest("tr");
            $(".modal label[for='codTicket']").show();
            $(".modal #codTicket").show();
            // Rellenar los campos del modal
            $(".modal h1").text("Modificar Ticket");
            $(".modal #formTituloTicket").val(tr.data("titulo"));
            $(".modal #codTicket").val(tr.data("codigo"));
            $(".modal #codTicket").prop("readonly", true);
            $(".modal #formDescripcionTicket").val(tr.data("descripcion"));
            $(".modal #formFechaCreacionTicket").val(tr.data("fechacreacion"));
            $(".modal #formIdEstado").val(tr.data("idEstado"));
            $(".modal #formIdPrioridad").val(tr.data("idPrioridad"));
            if (tr.data("idEmpleadoIt") === "") {
                $(".modal #formIdItResponsable").val(0);
            } else {
                $(".modal #formIdItResponsable").val(tr.data("idEmpleadoIt"));
            }
            $(".modal #idTicket").val(tr.data("idTicket"));

            if (tr.data("imagen") !== "") {
                showImageModal(tr.data("imagen"));
            }

        }
        else if (opcion === "cerrarTicket") {
            $("#modalActionButton").text("Cerrar");
            $("#modalActionButton").css({ "background-color": "#DC3545", "white-space": "nowrap" });
            $(".modal #accion").val("cerrarTicket");
            //ocultando los elementos que no son necesarios para esta operacion
            $(".modal .agregar-modificar").hide();
            $(".modal #reAbrir").hide();
            $(".modal #comenzar").hide();

            const codigoTicket = $(button).data("codticket");
            var tr = $(button).closest("tr");
            fechaCreacion = tr.data("fechacreacion");
            $(".modal #formTituloTicket").val(tr.data("titulo"));
            $("#modalBody #codTicket").val(codigoTicket);
            const today = new Date().toLocaleDateString('en-CA');
            $("#formfechaFinalizado").val(today);
            $(".modal h1").text("Cerrar Ticket");

        }
        else if (opcion === "reabrirTicket") {
            var tr = $(button).closest("tr");
            $(".modal #modalTitle").text("Reabrir Ticket");

            $(".modal .agregar-modificar").hide();
            $(".modal #comenzar").hide();
            $(".modal #reAbrir").show();

            $("#modalActionButton").text("Reabrir");
            $(".modal #accion").val("reabrirTicket");
            const codigoTicket = $(button).data("codticket"); // Esto debería funcionar correctamente ahora
            $("#modalBody #codTicket").val(codigoTicket);
            $(".modal #formTituloTicket").val(tr.data("titulo"));
            $(".modal #formTituloTicket").prop("readonly", "true");
            $(".modal #formfechaFinalizado").prop("readonly", "true");
            $(".modal #formfechaFinalizado").val(tr.data("fechafinalizado"));
        }
        else if (opcion === "comenzarTicket") {
            var tr = $(button).closest("tr");
            $(".modal #formfechaFinalizado").hide();
            $(".modal #reAbrir").hide();
            $(".modal label[for='formfechaFinalizado']").hide();
            $(".modal .agregar-modificar").hide();
            $(".modal #reAbrir").hide();

            $(".modal #accion").val("comenzarTicket");
            $(".modal #comenzar").text("¿Seguro que va a comenzar este ticket?");
            $(".modal #modalTitle").text("Comenzar Ticket");

            $(".modal #formTituloTicket").show();
            $(".modal #formTituloTicket").prop("readonly", "true");
            $(".modal #codTicket").show();
            $(".modal #comenzar").show();
            $(".modal label[for='formTituloTicket']").show();
            $(".modal label[for='codTicket']").show();

            $("#modalActionButton").text("Comenzar");
            $("#modalActionButton").css("background-color", "#d39800");

            const codigoTicket = $(button).data("codticket");
            $("#modalBody #codTicket").val(codigoTicket);
            $(".modal #formTituloTicket").val(tr.data("titulo"));
        }
    }
}

function previewImage(event) {
    var input = event.target;
    var reader = new FileReader();

    reader.onload = function () {
        var dataURL = reader.result;
        var preview = document.getElementById('previewImagenTicket');
        preview.src = dataURL;

        // Guardar base64 en el campo oculto
        document.getElementById('hiddenImagenBase64').value = dataURL.split(",")[1];  // Guardar solo el base64
    };

    reader.readAsDataURL(input.files[0]);
}

function showImageModal(imageData) {
    // Asegúrate de que la cadena base64 tenga el prefijo correcto
    var imgPrefix = "data:image/jpeg;base64,"; // O "data:image/png;base64," si es una imagen PNG

    // Si la cadena base64 no tiene el prefijo, agrégalo
    if (!imageData.startsWith("data:image")) {
        imageData = imgPrefix + imageData;
    }

    // Actualiza la imagen de vista previa con la data en base64
    document.getElementById('previewImagenTicket').src = imageData;

    // Muestra el campo oculto
    $(".modal #hiddenImagenBase64").show();

    // Establece el valor del campo oculto con la cadena base64
    document.getElementById('hiddenImagenBase64').value = imageData;
}
function submitFormTickets() {
    let estado = ($('#formIdEstado').val() === null || $('#formIdEstado').val() === undefined) ? null : $('#formIdEstado').val().trim() ;
    let empleado = ($('#formIdItResponsable').val() === null || $('#formIdItResponsable').val() === undefined) ? null : $('#formIdItResponsable').val();

    const extensionesDeImagenAceptadas = ["jpg", "JPG", "png", "PNG", "jpeg", "JPEG"];
    let extesionDeImagen = $("#formImagenTicket").val().trim().split(".").reverse()[0];

    let extensionValida = $.inArray(extesionDeImagen, extensionesDeImagenAceptadas) === -1 ? false : true;

    if ($(".modal #accion").val() === "cerrarTicket") {
        if ($("#formfechaFinalizado").val().trim() === "") {
            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Fecha finalizado es requerido!"
            });
            return false;
        } else if ((new Date(fechaCreacion) > new Date($("#formfechaFinalizado").val())) && (new Date(fechaCreacion).toLocaleDateString() !== new Date($("#formfechaFinalizado").val()).toLocaleDateString())) {
            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Fecha finalizado debe ser mayor a la fecha de creación!"
            });
            return false;

        } else if (new Date($("#formfechaFinalizado").val()) > new Date()) {
            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Fecha finalizado no puede ser posterior a este día!"
            });
            return false;

        }

        $("#formTickets").submit();

    } else if ($(".modal #accion").val() === "reabrirTicket") {
        $("#formTickets").submit();

    } else if ($(".modal #accion").val() === "comenzarTicket") {
        $("#formTickets").submit();

    } else if ($("#formTituloTicket").val().trim() === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Título es requerido!"
        });
        return false;

    } else if ($("#formDescripcionTicket").val().trim() === "" && ($(".modal #accion").val() != "cerrarTicket" || $(".modal #accion").val() != "reabrirTicket" || $(".modal #accion").val() != "comenzarTicket")) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Descripción es requerido!"
        });
        return false;

    } else if ($("#formIdPrioridad").val().trim() === "" && ($(".modal #accion").val() != "cerrarTicket" || $(".modal #accion").val() != "reabrirTicket" || $(".modal #accion").val() != "comenzarTicket")) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Prioridad es requerida!"
        });
        return false;
    } else if (estado && estado == "1" && (empleado == null || empleado == "")) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Asegúsere de elegir un IT responsable"
        });
        return false;
    } else if (estado && estado === "2" && (empleado == null || empleado != "")) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Asegúsere de no asignar un IT responsable"
        });
        return false;
    } else if ($("#formImagenTicket").val().trim() !== "" && !extensionValida) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "La extensión de la imagen solo puede ser PNG, JPG ó JPEG"
        });
        return false;
    }
    $("#formIdEstado").prop("disabled", false);
    $("#formTickets").submit();
}
