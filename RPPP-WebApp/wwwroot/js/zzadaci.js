$(document).on('click', '.deleterow', function (event) {
    event.preventDefault();
    var tr = $(this).parents("tr");
    tr.remove();
});

$("#newSuradnik").click(function () {
    var zadatakId = $("#newSuradnik").data("zadatak-id");

    var queryString = "?zadatakId=" + encodeURIComponent(zadatakId);
    var newUrl = "/Zsuradnik/Create" + queryString;
    window.location.href = newUrl;
});

$("#deleteButton").click(function () {
    if (window.confirm("Jeste li sigurni da želite izbrisati korisnika?")) {
        var suradnikId = $("#deleteButton").data("suradnik-id");
        var zadatakId = $("#newSuradnik").data("zadatak-id");

        var xhr = new XMLHttpRequest();

        xhr.open('POST', '/Zzadatak/DeleteSuradnik', true);
        xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');

        xhr.onload = function () {
            if (xhr.status === 200) {
                console.log('Uspešno obrisano');
            } else {
                console.error('Došlo je do greške prilikom brisanja');
            }
        };

        xhr.onerror = function () {
            console.error('Došlo je do greške prilikom slanja zahteva');
        };

        var formData = 'suradnikId=' + suradnikId + '&zadatakId=' + zadatakId;
        xhr.send(formData);
    }
});



$(function () {
    $(".form-control").bind('keydown', function (event) {
        if (event.which === 13) {
            event.preventDefault();
        }
    });

    $("#suradnik-Ime, #suradnik-Prezime, #suradnik-Email, #suradnik-BrojMobitela").bind('keydown', function (event) {
        if (event.which === 13) {
            event.preventDefault();
            dodajSuradnika();
        }
    });

    $("#suradnik-dodaj").click(function (event) {
        event.preventDefault();
        dodajSuradnika();
    });
});


function dodajSuradnika() {
    var suradnikId = $("#suradnik-SuradnikId").prop("value");
    console.log("SuradnikId je " + suradnikId);
    if (suradnikId != '') {
        if ($("[name='zsuradnikViewModels[" + suradnikId + "].SuradnikId']").length > 0) {
            alert('Suradnik je već u dokumentu');
            return;
        }

        var ime = $("#suradnik-Ime").val();
        var prezime = $("#suradnik-Prezime").val();
        var email = $("#suradnik-Email").val();
        var brMobitela = $("#suradnik-BrojMobitela").val();

        var template = `
            <tr>
                <td class="text-left col-sm-4">
                    <input type="hidden" name="zsuradnikViewModels.Index" value="${suradnikId}" />
                    <input type="text" hidden name="zsuradnikViewModels[${suradnikId}].Suradnik.SuradnikId" value="${suradnikId}" />
                    <input type="text" name="zsuradnikViewModels[${suradnikId}].Suradnik.Ime" value="${ime}" />
                </td>
                <td class="text-center col-sm-1">
                    <input type="text" name="zsuradnikViewModels[${suradnikId}].Suradnik.Prezime" value="${prezime}" />
                </td>
                <td class="text-center col-sm-1">
                    <input type="text" name="zsuradnikViewModels[${suradnikId}].Suradnik.Email" value="${email}" />
                </td>
                <td class="text-right col-sm-3">
                    <input type="text" name="zsuradnikViewModels[${suradnikId}].Suradnik.BrojMobitela" value="${brMobitela}" />
                </td>
                <td>
                    <button class="btn btn-sm btn-danger deleterow" title="Izbaci"><i class="fa fa-minus"></i>Izbaci</button>
                </td>
            </tr>`;

        $(template).insertBefore($("#table-suradnici tr:last"));

        $("#suradnik-Ime").val('');
        $("#suradnik-Prezime").val('');
        $("#suradnik-Email").val('');
        $("#suradnik-BrojMobitela").val('');
    } else {
        alert("Neispravan upit");
    }
}

