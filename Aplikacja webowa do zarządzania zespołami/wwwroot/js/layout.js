$(document).ready(function () {

    function toggleSidebarVisibility() {
        let sidebar = $("#sidebar");
        // Wyłączanie animacji podczas odczytywania danych
        sidebar.css("transition", "none");
        let isHidden = localStorage.getItem("sidebarHidden");
        if (isHidden === "true") {
            sidebar.css("width", "0");
        } else {
            sidebar.css("width", "280px");
        }
        // Włączanie z powrotem animacji po odczytaniu danych
        setTimeout(function () {
            sidebar.css("transition", "");
        }, 300);
    }

    // Zapisywanie stanu sideBar'a do local storage po kliknięciu przycisku
    $("#sidebarBtn").click(function () {
        let sidebar = $("#sidebar");
        let isHidden = sidebar.css("width") === "280px";
        localStorage.setItem("sidebarHidden", isHidden);
        sidebar.css("width", isHidden ? "0" : "280px");
    });

    toggleSidebarVisibility();

    //Zmiana klasy dla aktualnie wybranego elementu / strony
    $('#navbar .nav-link').each(function () {
        if ($(this).attr('href').toLowerCase() === location.pathname.toLowerCase()) {
            $(this).addClass('active');
        } else {
            $(this).removeClass('active');
        }
    });
});