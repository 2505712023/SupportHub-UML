function ModificarEmail(button) {
    var tr = $(button).closest("tr");
    let s = tr.data("ssl");
    $(".modal #esModificacionDeCorreo").val("true");
    $(".modal #correo").val(tr.data("correo"));
    $(".modal #asunto").val(tr.data("asunto"));
    $(".modal #cuerpo").val(tr.data("cuerpo"));
    $(".modal #host").val(tr.data("host"));
    $(".modal #puerto").val(tr.data("puerto"));
    $(".modal #contra").val(tr.data("contra"));
    $(".modal #ssl").val(s);

}

function limpiarModal() {
    $(".modal #esModificacionDeCorreo").val("false");
    $("#ModificarDatoMail")[0].reset();
}
function ValidarInfoCorreo() {
    let correo = $("#correo").val();
    let asunto = $("#asunto").val();
    let cuerpo = $("#cuerpo").val();
    let host = $("#host").val();
    let puerto = $("#puerto").val();
    let contra = $("#contra").val();
    let ssl = $("#ssl").val();

    if (correo == "") {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "La dirección del correo no puede estar vacío!"
        });
        return false;
    }
    else if (asunto == "") {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "El asunto no puede estar vacío!"
        });
        return false;
    } else if (cuerpo == "") {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "El cuerpo del correo no puede estar vacío!"
        });
        return false;
    } else if (host == "") {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "El host no puede estar vacío!"
        });
        return false;
    } else if (puerto == "") {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "El puerto no puede estar vacío!"
        });
        return false;
    } else if (contra == "") {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "La contraseña no puede estar vacía!"
        });
        return false;
    } else if (ssl == "") {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "El sertificado SSL no puede estar vacío!"
        });
        return false;
    }

    document.getElementById("ModificarDatoMail").submit();
}