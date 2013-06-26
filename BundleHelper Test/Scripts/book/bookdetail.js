/// <reference path="../jquery-1.7.1-vsdoc.js" />
$(document).ready(function () {
    $('#btnCheckStock').click(function () {
        $.post(
            CHECK_STOCK_URL, // refer to Views/Book/Detail.cshtml for detail
            {id: itemID}, // refer to Views/Book/Detail.cshtml for detail
            function (ret) {
                alert('In stock: ' + ret + ' item(s).');
            }
        );
    });
});