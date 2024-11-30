var swiper = new Swiper(".swiper", {
    effect: "cube",
    grabCursor: true,
    loop: true,
    speed: 1000,
    cubeEffect: {
        shadow: false,
        slideShadows: true,
        shadowOffset: 10,
        shadowScale: 0.94,
    },
    autoplay: {
        delay: 2600,
        pauseOnMouseEnter: true,
    },
});

tsParticles.load("tsparticles", {
    fpsLimit: 60,
    backgroundMode: {
        enable: true,
        zIndex: -1,
    },
    particles: {
        number: {
            value: 30,
            density: {
                enable: true,
                area: 800,
            },
        },
        color: {
            value: [
                "#3998D0",
                "#2EB6AF",
                "#A9BD33",
                "#FEC73B",
                "#F89930",
                "#F45623",
                "#D62E32",
            ],
        },
        // Resto de la configuración de partículas...
    },
    detectRetina: true,
});




