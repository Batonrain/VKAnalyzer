$(document).ready(function () {

    /* СОБЫТИЯ ПРИ ВЫБОРЕ ГОРОДОВ */
    var eventHandler_Auditory1_Cities = function (name) {
        return function () {
            $("#Auditory1_Cities").attr("value", arguments[0].join());
        };
    };
    var eventHandler_Auditory1_Exclude_Cities = function (name) {
        return function () {
            $("#Auditory1_ExcludeCities").attr("value", arguments[0].join());
        };
    };

    var eventHandler_Auditory2_Cities = function (name) {
        return function () {
            $("#Auditory2_Cities").attr("value", arguments[0].join());
        };
    };
    var eventHandler_Auditory2_Exclude_Cities = function (name) {
        return function () {
            $("#Auditory2_ExcludeCities").attr("value", arguments[0].join());
        };
    };
    /* СОБЫТИЯ ПРИ ВЫБОРЕ ГОРОДОВ */


    /* СОБЫТИЯ ПРИ ВЫБОРЕ ГРУПП РЕТАРГЕТА */
    var eventHandler_Auditory1_RetargetGroupIds = function (name) {
        return function () {
            $("#Auditory1_RetargetGroupIds").attr("value", arguments[0].join());
        };
    };
    var eventHandler_Auditory1_Exclude_RetargetGroupIds = function (name) {
        return function () {
            $("#Auditory1_ExcludeRetargetGroupIds").attr("value", arguments[0].join());
        };
    };

    var eventHandler_Auditory2_RetargetGroupIds = function (name) {
        return function () {
            $("#Auditory2_RetargetGroupIds").attr("value", arguments[0].join());
        };
    };
    var eventHandler_Auditory2_Exclude_RetargetGroupIds = function (name) {
        return function () {
            $("#Auditory2_ExcludeRetargetGroupIds").attr("value", arguments[0].join());
        };
    };
    /* СОБЫТИЯ ПРИ ВЫБОРЕ ГРУПП РЕТАРГЕТА */


    /* ЭЛЕМЕНТЫ ВЫБОРА ГОРОДОВ */
    var $selectize_Auditory1_Cities = $('#element_Auditory1_Cities').selectize({
        maxItems: null,
        valueField: 'id',
        labelField: 'title',
        searchField: 'title',
        options: [],
        create: true,
        onChange: eventHandler_Auditory1_Cities('onChange'),
    });
    var $selectize_Auditory1_Exclude_Cities = $('#element_Auditory1_Exclude_Cities').selectize({
        maxItems: null,
        valueField: 'id',
        labelField: 'title',
        searchField: 'title',
        options: [],
        create: true,
        onChange: eventHandler_Auditory1_Exclude_Cities('onChange'),
    });

    var $selectize_Auditory2_Cities = $('#element_Auditory2_Cities').selectize({
        maxItems: null,
        valueField: 'id',
        labelField: 'title',
        searchField: 'title',
        options: [],
        create: true,
        onChange: eventHandler_Auditory2_Cities('onChange'),
    });
    var $selectize_Auditory2_Exclude_Cities = $('#element_Auditory2_Exclude_Cities').selectize({
        maxItems: null,
        valueField: 'id',
        labelField: 'title',
        searchField: 'title',
        options: [],
        create: true,
        onChange: eventHandler_Auditory2_Exclude_Cities('onChange'),
    });
    /* ЭЛЕМЕНТЫ ВЫБОРА ГОРОДОВ */

    /* ЭЛЕМЕНТЫ ВЫБОРА ГРУПП РЕТАРГЕТА */
    var $selectize_Auditory1_RetargetGroupIds = $('#element_Auditory1_RetargetGroupIds').selectize({
        maxItems: null,
        valueField: 'id',
        labelField: 'title',
        searchField: 'title',
        options: [],
        create: true,
        onChange: eventHandler_Auditory1_RetargetGroupIds('onChange'),
    });
    var $selectize_Auditory1_Exclude_RetargetGroupIds = $('#element_Auditory1_Exclude_RetargetGroupIds').selectize({
        maxItems: null,
        valueField: 'id',
        labelField: 'title',
        searchField: 'title',
        options: [],
        create: true,
        onChange: eventHandler_Auditory1_Exclude_RetargetGroupIds('onChange'),
    });

    var $selectize_Auditory2_RetargetGroupIds = $('#element_Auditory2_RetargetGroupIds').selectize({
        maxItems: null,
        valueField: 'id',
        labelField: 'title',
        searchField: 'title',
        options: [],
        create: true,
        onChange: eventHandler_Auditory2_RetargetGroupIds('onChange'),
    });
    var $selectize_Auditory2_Exclude_RetargetGroupIds = $('#element_Auditory2_Exclude_RetargetGroupIds').selectize({
        maxItems: null,
        valueField: 'id',
        labelField: 'title',
        searchField: 'title',
        options: [],
        create: true,
        onChange: eventHandler_Auditory2_Exclude_RetargetGroupIds('onChange'),
    });
    /* ЭЛЕМЕНТЫ ВЫБОРА ГРУПП РЕТАРГЕТА */

    $(function () {
        $.getJSON("/Vk/GetCities")
        .done(function (data) {
            var control_Auditory1_Cities = $selectize_Auditory1_Cities[0].selectize;
            var control_Auditory1_Exclude_Cities = $selectize_Auditory1_Exclude_Cities[0].selectize;

            var control_Auditory2_Cities = $selectize_Auditory2_Cities[0].selectize;
            var control_Auditory2_Exclude_Citiess = $selectize_Auditory2_Exclude_Cities[0].selectize;

            $.each(data.response, function (index, value) {
                    control_Auditory1_Cities.addOption({
                        id: value.id,
                        title: value.name
                    });
                    control_Auditory1_Exclude_Cities.addOption({
                        id: value.id,
                        title: value.name
                    });

                    control_Auditory2_Cities.addOption({
                        id: value.id,
                        title: value.name
                    });
                    control_Auditory2_Exclude_Citiess.addOption({
                        id: value.id,
                        title: value.name
                    });
            });
        });
    });

    $(function () {
        disableAll();
        disableRetargetGroups();
        $.getJSON("/Vk/GetAccounts")
        .done(function (data) {
            $.each(data.response, function (index, value) {
                var accountOption = $('<option>');
                accountOption.attr('value', value.account_id)
                             .text(value.account_name);

                if (index === 0) {
                    $("#AccountId").attr("value", value.account_id);

                    // Получение клиентов для рекламного кабинета
                    getClients(value.account_id);
                }

                $('#AccountNames').append(accountOption);
            });
        });
    });

    function getClients(accountId) {
        disableRetargetGroups();
        $.getJSON("/Vk/GetClients", { accountId: accountId })
            .done(function (clients) {
                $('#Clients').find('option').remove().end();

                var needToDisableClient = false;

                if (clients.error) {
                    $("#ClientId").attr("value", "");
                    // Получение групп ретаргета напрямую
                    needToDisableClient = true;
                    getRetargets(accountId);
                } else {
                    $.each(clients.response, function (cIndex, cValue) {
                        if (cIndex === 0) {
                            $("#ClientId").attr("value", cValue.id);
                            // Получение групп ретаргета напрямую
                            getRetargets(accountId, cValue.id);
                        }

                        var clientOption = $('<option>');
                        clientOption.attr('value', cValue.id)
                                   .text(cValue.name);

                        $('#Clients').append(clientOption);
                    });
                }

                enableAll(needToDisableClient);
            });
    }

    function getRetargets(accountId, clientId) {
        $.getJSON("/Vk/GetTargetGroups", { accountId: accountId, clientId: clientId })
            .done(function (groups) {
                var control_Auditory1_RetargetGroupIds = $selectize_Auditory1_RetargetGroupIds[0].selectize;
                var control_Auditory1_Exclude_RetargetGroupIds = $selectize_Auditory1_Exclude_RetargetGroupIds[0].selectize;

                var control_Auditory2_RetargetGroupIds = $selectize_Auditory2_RetargetGroupIds[0].selectize;
                var control_Auditory2_Exclude_RetargetGroupIds = $selectize_Auditory2_Exclude_RetargetGroupIds[0].selectize;

                $.each(groups.response, function (gIndex, gValue) {
                    control_Auditory1_RetargetGroupIds.addOption({
                        id: gValue.id,
                        title: gValue.name
                    });
                    control_Auditory1_Exclude_RetargetGroupIds.addOption({
                        id: gValue.id,
                        title: gValue.name
                    });

                    control_Auditory2_RetargetGroupIds.addOption({
                        id: gValue.id,
                        title: gValue.name
                    });
                    control_Auditory2_Exclude_RetargetGroupIds.addOption({
                        id: gValue.id,
                        title: gValue.name
                    });
                });
            });

        enableRetargetGroups();
    }

    $("#AccountNames").change(function () {
        $("#AccountId").attr("value", $(this).val());

        $("#Clients").prop('disabled', true);

        getClients($(this).val());
    });

    $("#Clients").change(function () {
        $("#ClientId").attr("value", $(this).val());
    });

    function disableAll() {
        $("#AccountNames").prop('disabled', true);
        $("#Clients").prop('disabled', true);
    }

    function enableAll(needToDisableClient) {
        $("#AccountNames").prop('disabled', false);
        $("#Clients").prop('disabled', needToDisableClient);
    }

    function disableClient() {
        $("#Clients").prop('disabled', true);
    }

    function disableRetargetGroups() {
        $selectize_Auditory1_RetargetGroupIds[0].selectize.disable();
        $selectize_Auditory1_Exclude_RetargetGroupIds[0].selectize.disable();
        $selectize_Auditory2_RetargetGroupIds[0].selectize.disable();
        $selectize_Auditory2_Exclude_RetargetGroupIds[0].selectize.disable();
    }

    function enableRetargetGroups() {
        $selectize_Auditory1_RetargetGroupIds[0].selectize.enable();
        $selectize_Auditory1_Exclude_RetargetGroupIds[0].selectize.enable();
        $selectize_Auditory2_RetargetGroupIds[0].selectize.enable();
        $selectize_Auditory2_Exclude_RetargetGroupIds[0].selectize.enable();
    }

});