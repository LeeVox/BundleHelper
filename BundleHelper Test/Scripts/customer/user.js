/// <reference path="../jquery-1.7.1-vsdoc.js" />
$(document).ready(function () {
    $('#btnLogout').click(logout);
});

function logout() {
    alert('You have just clicked on button Log out.');
}