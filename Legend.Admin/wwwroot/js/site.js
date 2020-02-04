// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var paginationPage = parseInt($('.cdp').attr('actpage'), 10);
$('.cdp_i').on('click', function () {
    var go = $(this).attr('href').replace('#!', '');
    if (go === '+1') {
        paginationPage++;
    } else if (go === '-1') {
        paginationPage--;
    } else {
        paginationPage = parseInt(go, 10);
    }
    $('.cdp').attr('actpage', paginationPage);
});