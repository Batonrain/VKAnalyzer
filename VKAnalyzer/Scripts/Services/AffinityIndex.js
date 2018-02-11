$(document).ready(function () {
    $(function () {
        disableCities();

        $.getJSON("/Vk/GetCities")
        .done(function (data) {
            console.log(JSON.stringify(data.response));
            $.each(data.response, function (index, value) {
                var citiesOption = document.createElement('option');
                citiesOption.value = value.id;
                citiesOption.text = value.name;

                if (index === 0) {
                    $("#Auditory1_City").attr("value", value.id);
                    $("#Auditory2_City").attr("value", value.id);
                }

                $('#CityName1').append(citiesOption.cloneNode(true));
                $('#CityName2').append(citiesOption.cloneNode(true));
            });
        });

        enableCities();
    });

    $(function () {
        disableCities();

        $.getJSON("/Vk/GetCities")
        .done(function (data) {
            console.log(JSON.stringify(data.response));
            $.each(data.response, function (index, value) {
                var citiesOption = document.createElement('option');
                citiesOption.value = value.id;
                citiesOption.text = value.name;

                if (index === 0) {
                    $("#Auditory1_City").attr("value", value.id);
                    $("#Auditory2_City").attr("value", value.id);
                }

                $('#CityName1').append(citiesOption.cloneNode(true));
                $('#CityName2').append(citiesOption.cloneNode(true));
            });
        });

        enableCities();
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
        $.getJSON("/Vk/GetClients", { accountId: accountId })
            .done(function (clients) {
                $('#Clients').find('option').remove().end();

                var needToDisableClient = false;

                if (clients.error) {
                    $("#ClientId").attr("value", "");
                    // Получение групп ретаргета напрямую
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
                        if (!cValue.id)
                            needToDisableClient = true;
                    });
                }

                enableAll(needToDisableClient);
                enableRetargetGroups();
            });
    }

    function getRetargets(accountId, clientId) {
        $.getJSON("/Vk/GetTargetGroups", { accountId: accountId, clientId: clientId })
                     .done(function (groups) {

                         $('#RetargetGroupIds1').find('option').remove().end();

                         $.each(groups.response, function (gIndex, gValue) {
                             if (gIndex === 0) {
                                 $("#Auditory1_RetargetGroupId").attr("value", gValue.id);
                             }
                             var groupOption = $('<option>');
                             groupOption.attr('value', gValue.id)
                                        .text(gValue.name);
                             $('#RetargetGroupIds1').append(groupOption);
                         });

                         var needToDisableClient = false;
                         if (!clientId)
                             needToDisableClient = true;

                         enableAll(needToDisableClient);
                     });
    }

    $("#AccountNames").change(function () {
        $("#AccountId").attr("value", $(this).val());

        $("#Clients").prop('disabled', true);

        getClients($(this).val());
    });

    $("#Clients").change(function () {
        $("#ClientId").attr("value", $(this).val());
    });

    $("#RetargetGroupIds1").change(function () {
        $("#Auditory1_RetargetGroupId").attr("value", $(this).val());
    });

    $("#RetargetGroupIds2").change(function () {
        $("#Auditory2_RetargetGroupId").attr("value", $(this).val());
    });

    $("#CityName1").change(function () {
        $("#Auditory1_City").attr("value", $(this).val());
    });

    $("#CityName2").change(function () {
        $("#Auditory2_City").attr("value", $(this).val());
    });

    function disableAll() {
        $("#AccountNames").prop('disabled', true);
        $("#Clients").prop('disabled', true);
    }

    function enableAll(disableClient) {
        $("#AccountNames").prop('disabled', false);
        $("#Clients").prop('disabled', disableClient);
    }

    function disableRetargetGroups() {
        $("#RetargetGroupIds1").prop('disabled', true);
        $("#RetargetGroupIds2").prop('disabled', true);
    }

    function enableRetargetGroups() {
        $("#RetargetGroupIds1").prop('disabled', false);
        $("#RetargetGroupIds2").prop('disabled', false);
    }

    function disableCities() {
        $("#CityName1").prop('disabled', true);
        $("#CityName2").prop('disabled', true);
    }

    function enableCities() {
        $("#CityName1").prop('disabled', false);
        $("#CityName2").prop('disabled', false);
    }
});