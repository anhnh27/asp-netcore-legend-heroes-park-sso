$(document).ready(function () {
    $('#btn-reset').on("click", () => {
        var email = $('#email').val();

        var URL = ApplicationUrl + "/api/Account/requestpasswordresetemail/" + email;

        $.ajax(
            {
                url: URL,
                type: "GET",
                success: function (data) {
                    if (data.accountExist) {
                        alert(data.message);
                        window.history.back();
                    } else {
                        alert(data.message);
                    }
                },
                error: function (error) {
                    alert(error.message);
                    window.history.back();
                }
            }
        );
    });
});