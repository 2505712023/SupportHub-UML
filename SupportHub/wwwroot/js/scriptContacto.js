document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("contactForm");

    form.addEventListener("submit", function (event) {
        event.preventDefault(); // Evita el envío y redirección predeterminada

        // Enviar el formulario a Formspree sin redirección
        fetch("https://formspree.io/f/xkgnjevj", {
            method: "POST",
            body: new FormData(form),
            headers: {
                'Accept': 'application/json'
            }
        }).then(response => {
            if (response.ok) {
                // Limpiar los campos del formulario
                form.reset();
                // Mostrar confirmación con SweetAlert
                Swal.fire({
                    icon: 'success',
                    title: '¡Enviado!',
                    text: 'Tu mensaje ha sido enviado, pronto nos contactaremos contigo.',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'Aceptar'
                });
            } else {
                // Mostrar error con SweetAlert
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Hubo un problema al enviar tu mensaje. Intenta nuevamente.',
                    confirmButtonColor: '#d33',
                    confirmButtonText: 'Aceptar'
                });
            }
        }).catch(error => {
            // Manejo del error en caso de que fetch falle
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Hubo un problema de conexión. Por favor, intenta más tarde.',
                confirmButtonColor: '#d33',
                confirmButtonText: 'Aceptar'
            });
        });
    });
});

document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("whatsappButton").addEventListener("click", function () {
        var phoneNumber = "50377442775"; // Tu número de teléfono con código de país
        var message = "Hola, me gustaría más información."; // El mensaje predefinido
        var url = "https://wa.me/" + phoneNumber + "?text=" + encodeURIComponent(message);
        window.open(url, "_blank"); // Abre el enlace en una nueva pestaña
    });
});
