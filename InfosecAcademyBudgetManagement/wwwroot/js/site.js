// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

window.showSwalWarning = function (message, title) {
    if (!message) {
        return;
    }

    if (typeof Swal === "undefined") {
        alert(message);
        return;
    }

    Swal.fire({
        icon: "warning",
        title: title || "Uyarı",
        text: message,
        confirmButtonText: "Tamam"
    });
};
