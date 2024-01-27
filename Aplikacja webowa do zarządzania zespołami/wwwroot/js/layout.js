$(document).ready(function () {

    function toggleSidebarVisibility() {
        let sidebar = $("#sidebar");
        // Turn off animation for initial loading
        sidebar.css("transition", "none");
        let isHidden = localStorage.getItem("sidebarHidden");
        if (isHidden === "true") {
            sidebar.css("width", "0");
        } else {
            sidebar.css("width", "280px");
        }
        // Turn back animation (set more than time of animation to prevent it from randomly fireing)
        setTimeout(function () {
            sidebar.css("transition", "");
        }, 300);
    }

    $("#sidebarBtn").click(function () {
        let sidebar = $("#sidebar");
        let isHidden = sidebar.css("width") === "280px";
        localStorage.setItem("sidebarHidden", isHidden);
        sidebar.css("width", isHidden ? "0" : "280px");
    });

    toggleSidebarVisibility();

    //Change class for selected page
    $('#navbar .nav-link').each(function () {
        if ($(this).attr('href').toLowerCase() === location.pathname.toLowerCase()) {
            $(this).addClass('active');
        } else {
            $(this).removeClass('active');
        }
    });
});