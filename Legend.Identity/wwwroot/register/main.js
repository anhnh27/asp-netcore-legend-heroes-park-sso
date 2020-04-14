function uploadFile(input) {
    var files = input.files;
    var formData = new FormData();
    
    for (var i = 0; i !== files.length; i++) {
        formData.append('file', $('#photo')[0].files[0]);
    }

    var URL = ApplicationUrl + "/api/Account/Upload";
    
    $.ajax(
        {
            url: URL,
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            success: function (data) {
                $('#picture').val(data.FileRelativeUrl);
            }
        }
    );
}

function readURL(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#photo-preview').attr('src', e.target.result);
        };

        reader.readAsDataURL(input.files[0]);
    }
}

$(document).ready(function () {
    $('#birthday').datepicker();
    $('#photo').change(function () {
        readURL(this);
        uploadFile(this);
    });
});