function validarFormulario() {
    var codigo = document.getElementById("codigo").value.trim();
    var nombre = document.getElementById("nombre").value.trim();
    var direccion = document.getElementById("direccion").value.trim();
    var telefono = document.getElementById("telefono").value.trim();

    if (codigo === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Código es requerido!"
        });
        return false;
    } else if (nombre === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Nombre es requerido!"
        });
        return false;
    } else if (direccion === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Dirección es requerido!"
        });
        return false;
    } else if (direccion.length > 100) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Dirección solo permite 100 caracteres!"
        });
        return false;
    } else if (iti && !iti.isValidNumber()) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Teléfono es incorrecto!"
        });
        return false;
    }
    submitForm();
}

function validarFormularioEliminar() {
    if ($(".modal#Eliminar #idProveedor").val().trim() === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "idProveedor es requerido!"
        });
        return false;
    }
    submitFormEliminar()
}
//funciones de la página mi_informacion

document.addEventListener('DOMContentLoaded',ModalModificarContraseña);

function ModalModificarContraseña(button) {
    var tr = $(button).closest("tr");

    if (tr.data("modificarusuario") === "true") {
        $(".modal h1").text("Modificar Datos");
    }

    $("#id").val(tr.data("id"));
    $(".modal #usuario").val(tr.data("usuario"));
}

document.addEventListener('DOMContentLoaded', ModalModificarCorreo);

function ModalModificarCorreo(button) {
    var tr = $(button).closest("tr");

    $(".modal h1").text("Modificar datos");
    $("#id").val(tr.data("id"));
    $(".modal #enviosCorreo").val(tr.data("enviosCorreo"));
}


function validarIformacionUsuario() {
    var usuario = document.getElementById("usuario").value.trim();
    var contra = document.getElementById("contraA").value.trim();
    var contraLogueada = sessionStorage.getItem("contra");
    var nuevaContra = document.getElementById("nContra").value.trim();
    var confirmarNuevaContra = document.getElementById("CnContra").value.trim();

    if (usuario === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "El nombre de usuario no puede estar vacío!"
        });
        return false;
    } else if (contra === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "La contraseña actual no puede estar vacía!"
        });
        return false;
    } else if (contra != contraLogueada) {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "La contraseña actual es incorrecta!"
        });
        return false;
    } else if (nuevaContra != "" && confirmarNuevaContra == "" || nuevaContra == "" && confirmarNuevaContra != "") {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Por favor, complete ambos campos de nueva contraseña."
        });
        return false;
    } else if (nuevaContra !== confirmarNuevaContra) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "La nueva contraseña no coincide!"
        });
        return false;
    } else if (confirmarNuevaContra != "" && !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/.test(nuevaContra)) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "La nueva contraseña debe tener al menos 8 caracteres, incluyendo mayúsculas, minúsculas, números y caracteres especiales."
        });
        return false;
    }
    submitFormModificarInfoUsuario();
}

function submitFormModificarInfoUsuario() {
    document.getElementById("ModificarinfoUsuario").submit();
}
// mostrar y ocultar la contraseña
const input1 = document.getElementById("nContra"),
    input2 = document.getElementById("CnContra"),
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
function submitForm() {
    document.getElementById("formAgregarProveedor").submit();
}

function submitFormEliminar() {
    document.getElementById("formEliminarProveedor").submit();
}

function checkSearch(input) {
    if (input.value.trim() === '') {
        input.form.submit();
    }
}

function llenarModal(button) {
    var tr = $(button).closest("tr");

    $(".modal #esModificacion").val("true")
    $(".modal h1").text("Modificar Proveedor");
    $(".modal #id").val(tr.data("id"));
    $(".modal #codigo").val(tr.data("codigo"));
    $(".modal #nombre").val(tr.data("nombre"));
    $(".modal #direccion").val(tr.data("direccion"));
    $(".modal #telefono").val(tr.data("telefono"));

    $(".modal #codigo").prop("readonly", true);

    // Configuración del prefijo de país
    const phoneInput = document.querySelector("#telefono");
    const iti = window.intlTelInputGlobals.getInstance(phoneInput); // Obtenemos la instancia de intl-tel-input

    // Establecer el prefijo de país basándose en el prefijo del teléfono guardado
    iti.setNumber(tr.data("telefono")); // Esto ajustará el país según el número completo

    $(".modal #codigo").prop("readonly", true);
}

//funciones para modificar usuario
function limpiarModal() {
    $(".modal #esModificacion").val("false");
    $(".modal h1").text("Agregar Proveedor");
    $("#formAgregarProveedor")[0].reset();

    $(".modal #codigo").prop("readonly", false);
}

function llenarModalEliminar(button) {
    var tr = $(button).closest("tr");

    $(".modal#Eliminar #idProveedor").val(tr.data("id"));
    $(".modal h1").text("Eliminar Proveedor");
    $(".modal#Eliminar #esEliminacion").val("true");
    $("#Eliminar").on("shown.bs.modal", function () {
        $("#btnEliminar").focus();
    });
}

function limpiarModalEliminar() {
    $(".modal#Eliminar #esEliminacion").val("false");
    $(".modal h1").text("Agregar Proveedor");
    $("#formEliminarProveedor")[0].reset();
}

//permitir valores correctos para el campo de teléfono
let input = document.getElementById("telefono");
if (input) {
    function soloPermitidos(event) {
        const permitidos = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "-", " ", "Backspace", "Delete", "ArrowLeft", "ArrowRight"];
        if (!permitidos.includes(event.key)) {
            event.preventDefault();
        }
    }
    input.addEventListener('keydown', soloPermitidos);
}

function limpiarModalMUsuario() {

    $("#ModificarinfoUsuario")[0].reset();
}


function ModalModificarUsuario(button) {
    var tr = $(button).closest("tr");

    $(".modal h1").text("Modificar Mi Información");
    $("#id").val(tr.data("id"));
    $("#usuario").val(tr.data("usuario"));

    // ocultando los input y label que no se van a usar en esta operación
    $("#nContra").attr("type", "hidden");
    $("#CnContra").attr("type", "hidden");
    $('label[for="nContra"]').hide();
    $('label[for="CnContra"]').hide();
}

function validaInfoTicket() {
    var usuario = document.getElementById("usuario").value.trim();
    var contra = document.getElementById("contraA").value.trim();
    var contraLogueada = sessionStorage.getItem("contra");
    var nuevaContra = document.getElementById("nContra").value.trim();
    var confirmarNuevaContra = document.getElementById("CnContra").value.trim();

    if (usuario === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "El nombre de usuario no puede estar vacío!"
        });
        return false;
    } else if (contra === "") {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "La contraseña actual no puede estar vacía!"
        });
        return false;
    } else if (contra != contraLogueada) {

        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "La contraseña actual es incorrecta!"
        });
        return false;
    } else if (nuevaContra != "" && confirmarNuevaContra == "" || nuevaContra == "" && confirmarNuevaContra != "") {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Por favor, complete ambos campos de nueva contraseña."
        });
        return false;
    } else if (nuevaContra !== confirmarNuevaContra) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "La nueva contraseña no coincide!"
        });
        return false;
    } else if (confirmarNuevaContra != "" && !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/.test(nuevaContra)) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "La nueva contraseña debe tener al menos 8 caracteres, incluyendo mayúsculas, minúsculas, números y caracteres especiales."
        });
        return false;
    }
    submitFormModificarInfoUsuario();
}