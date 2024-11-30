document.addEventListener('DOMContentLoaded', function () {
    var toggleButton = document.getElementById('toggle-theme');
    var darkModeEnabled = localStorage.getItem('dark-mode') === 'true';

    if (darkModeEnabled) {
        document.body.classList.add('dark-mode');
        var icon = document.querySelector('#toggle-theme i');
        icon.classList.replace('fa-moon', 'fa-sun');
    }

    if (toggleButton) {
        toggleButton.addEventListener('click', function () {
            var isDarkMode = document.body.classList.toggle('dark-mode');
            localStorage.setItem('dark-mode', isDarkMode);
            var icon = document.querySelector('#toggle-theme i');
            if (isDarkMode) {
                icon.classList.replace('fa-moon', 'fa-sun');
            } else {
                icon.classList.replace('fa-sun', 'fa-moon');
            }
        });
    }
});
