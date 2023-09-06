$(document).ready(function () {

    $(document).ready(function () {
        $("#sidebarBtn").click(function () {
            let sidebar = $("#sidebar");
            if (sidebar.css("width") === "280px") {
                sidebar.css("width", "0");

            } else {
                sidebar.css("width", "280px");
            }
        });
    });

    //Zmiana klasy dla aktualnie wybranego elementu / strony
    $('#navbar .nav-link').each(function () {
        if ($(this).attr('href').toLowerCase() === location.pathname.toLowerCase()) {
            $(this).addClass('active');
        } else {
            $(this).removeClass('active');
        }
    });
});