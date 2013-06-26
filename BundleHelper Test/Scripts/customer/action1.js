/// <reference path="../jquery-1.7.1-vsdoc.js" />
$(document).ready(function () {
    $('button').click(function () {
        alert('You have clicked on button "' + $(this).text() + '"');
    });
});