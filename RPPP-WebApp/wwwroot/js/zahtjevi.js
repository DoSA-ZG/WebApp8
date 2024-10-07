$(document).on('click', '.deleterow', function () {
    event.preventDefault();
    var tr = $(this).parents("tr");
    tr.remove();
    clearOldMessage();
});

$(function () {
    $(".form-control").bind('keydown', function (event) {
        if (event.which === 13) {
            event.preventDefault();
        }
    });


    $("#zadatak-dodaj").click(function () {
        event.preventDefault();
        dodajZadatak();
    });
});


// TODO!!
function dodajZadatak() {
    var StatusZadatkaId = $("#zadatak-StatusZadatkaId").val();
    var Opis = $("#zadatak-opis").val();
    console.log(StatusZadatkaId);
    if (StatusZadatkaId != '' && Opis != '') {

        var template = $('#template').html();
        
        var StatusZadatka = $("#zadatak-StatusZadatka").val();

        template = template
            .replace(/--StatusZadatkaId--/g, StatusZadatkaId)
            .replace(/--StatusZadatka--/g, StatusZadatka)
            .replace(/--Opis--/g, Opis)
        $(template).find('tr').insertBefore($("#table-zadatci").find('tr').last());

        $("#zadatak-StatusZadatka").val('');
        $("#zadatak-opis").val('');

        clearOldMessage();
    }
}