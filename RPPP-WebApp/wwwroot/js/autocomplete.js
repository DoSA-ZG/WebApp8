$(function () {
    $("[data-autocomplete]").each(function (index, element) {
        var action = $(element).data('autocomplete');
        var controller = $(element).data('controller');

        // Check if data-controller attribute is present, otherwise default to "autocomplete"
        if (typeof controller === 'undefined' || controller === null) {
            controller = "autocomplete";
        }

        var resultplaceholder = $(element).data('autocomplete-placeholder-name');
        if (typeof resultplaceholder === 'undefined') {
            resultplaceholder = action;
        }

        $(element).change(function () {
            var dest = $(`[data-autocomplete-placeholder='${resultplaceholder}']`);
            var text = $(element).val();
            if (text.length === 0 || text !== $(dest).data('selected-label')) {
                $(dest).val('');
            }
        });

        $(element).autocomplete({
            source: window.applicationBaseUrl + controller + "/" + action,
            autoFocus: true,
            minLength: 1,
            select: function (event, ui) {
                $(element).val(ui.item.label);
                var dest = $(`[data-autocomplete-placeholder='${resultplaceholder}']`);
                $(dest).val(ui.item.id);
                $(dest).data('selected-label', ui.item.label);
            }
        });
    });
});