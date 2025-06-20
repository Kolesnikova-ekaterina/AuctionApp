// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function showAlert(message) {
    // Пример с Toast-уведомлением
    Toastify({
        text: `ALERT: ${message}`,
        duration: 5000,
        gravity: "top",
        position: "right",
        backgroundColor: "linear-gradient(to right, #ff5f6d, #ffc371)",
    }).showToast();
    
    // Или вариант с alert()
    // alert(`Оповещение: ${message}`);
}