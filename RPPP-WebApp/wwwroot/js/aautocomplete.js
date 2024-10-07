$(function () {
    $("[data-autocomplete]").each(function (index, element) {
        var action = $(element).data('autocomplete');
        var resultplaceholder = $(element).data('autocomplete-placeholder-name');
        if (resultplaceholder === undefined)
            resultplaceholder = action;

        $(element).change(function () {
            var dest = $(`[data-autocomplete-placeholder='${resultplaceholder}']`);
            var text = $(element).val();
            if (text.length === 0 || text !== $(dest).data('selected-label')) {
                $(dest).val('');
            }
        });

        $(element).autocomplete({
            source: window.applicationBaseUrl + "autocomplete/" + action,
            autoFocus: true,
            minLength: 1,
            select: function (event, ui) {
                $(element).val(ui.item.Ime); // Set only the name
                var dest = $(`[data-autocomplete-placeholder='${resultplaceholder}']`);
                $(dest).val(ui.item.id);
                $(dest).data('selected-label', ui.item.Ime); // Use only the name

                // Update corresponding input fields in the same row
                var row = $(element).closest('tr');
                row.find("#suradnik-Ime").val(ui.item.Ime);
                row.find("#suradnik-Prezime").val(ui.item.Prezime);
                row.find("#suradnik-Email").val(ui.item.Email);
                row.find("#suradnik-BrojMobitela").val(ui.item.BrojMobitela);

                return false; // Prevent default behavior
            }
        });
    });
});
